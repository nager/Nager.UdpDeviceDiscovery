using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace Nager.UdpDeviceDiscovery
{
    /// <summary>
    /// Udp DeviceDetector
    /// </summary>
    public class UdpDeviceDetector
    {
        private readonly ILogger<UdpDeviceDetector> _logger;
        private readonly INetworkInterfaceRepository _networkInterfaceDetection;

        public event Action<DeviceInfoPackage> DeviceResponseReceived;

        /// <summary>
        /// Udp DeviceDetector
        /// </summary>
        /// <param name="logger"></param>
        /// <param name="networkInterfaceRepository"></param>
        public UdpDeviceDetector(
            ILogger<UdpDeviceDetector> logger = default,
            INetworkInterfaceRepository networkInterfaceRepository = default)
        {
            this._logger = logger == default ? new NullLogger<UdpDeviceDetector>() : logger;

            if (networkInterfaceRepository == default)
            {
                this._networkInterfaceDetection = new AutoDetectNetworkInterfaceRepository();
                return;
            }

            this._networkInterfaceDetection = networkInterfaceRepository;
        }

        /// <summary>
        /// Send a broadcast package to the network and wait for an answer
        /// </summary>
        /// <param name="deviceListeningPort">The port on the device where requests are responded to</param>
        /// <param name="deviceHelloPackage">The hello bytes where the device expects</param>
        /// <param name="hostSendPort">The send port of the host, 0 = automatic select next free udp port</param>
        /// <param name="deviceResponsePort">The port of the response</param>
        /// <param name="responseMustBeSameInterface">Response must be received at the same interface, on a Docker container, the host receives the package and not the container</param>
        /// <param name="receiveTimeout">The waiting time for a response after the hello package was sent (milliseconds)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task ScanAsync(
            int deviceListeningPort,
            byte[] deviceHelloPackage,
            int hostSendPort = 0,
            DeviceResponsePort deviceResponsePort = DeviceResponsePort.SendPort,
            bool responseMustBeSameInterface = true,
            int receiveTimeout = 1000,
            CancellationToken cancellationToken = default)
        {
            var tasks = new List<Task>();

            var networkInterfaces = this._networkInterfaceDetection.GetNetworkInterfaces();
            foreach (var networkInterface in networkInterfaces)
            {
                var broadcastTask = Task.Run(async () =>
                {
                    var localEndpoint = new IPEndPoint(IPAddress.Parse(networkInterface.IpAddress), hostSendPort);

                    this._logger.LogDebug($"{nameof(ScanAsync)} - Start broadcast {networkInterface.IpAddress}");
                    using (var udpClient = new UdpClient { ExclusiveAddressUse = false })
                    {
                        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
                        udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);
                        udpClient.Client.Bind(localEndpoint);

                        IPAddress receiveIpAddress;
                        IPEndPoint receiveEndPoint;

                        if (responseMustBeSameInterface)
                        {
                            receiveIpAddress = IPAddress.Parse(networkInterface.IpAddress);
                        }
                        else
                        {
                            receiveIpAddress = IPAddress.Any;
                        }

                        switch (deviceResponsePort)
                        {
                            case DeviceResponsePort.SendPort:
                                var sendPort = ((IPEndPoint)udpClient.Client.LocalEndPoint).Port;
                                receiveEndPoint = new IPEndPoint(receiveIpAddress, hostSendPort);
                                break;
                            case DeviceResponsePort.ListeningPort:
                                receiveEndPoint = new IPEndPoint(receiveIpAddress, deviceListeningPort);
                                break;
                            default:
                                throw new NotImplementedException($"Not Implemented option {deviceResponsePort}");
                        }

                        void dataReveived(byte[] data, IPAddress ipAddress)
                        {
                            var package = new DeviceInfoPackage
                            {
                                DiscoveredNetwork = networkInterface,
                                DeviceIpAddress = ipAddress.ToString(),
                                ReceivedData = data
                            };

                            this.DeviceResponseReceived?.Invoke(package);
                        }

                        using (var receiver = new UdpReceiver(this._logger, receiveEndPoint))
                        {
                            receiver.DataReceived += dataReveived;
                            _ = Task.Run(async () => await receiver.ReceiveAsync().ConfigureAwait(false));

                            this._logger.LogInformation("Send hello message");
                            await this.SendAsync(udpClient, deviceListeningPort, deviceHelloPackage).ConfigureAwait(false);

                            await Task.Delay(receiveTimeout, cancellationToken).ContinueWith(task => { }).ConfigureAwait(false);
                            receiver.DataReceived -= dataReveived;
                        }
                    }
                }, cancellationToken);

                tasks.Add(broadcastTask);
            }

            await Task.WhenAll(tasks).ConfigureAwait(false);
        }

        /// <summary>
        /// Send a broadcast package to the network and wait for an answer
        /// </summary>
        /// <param name="deviceListeningPort">The port on the device where requests are responded to</param>
        /// <param name="deviceHelloPackage">The hello bytes where the device expects</param>
        /// <param name="hostSendPort">The send port of the host, 0 = automatic select next free udp port</param>
        /// <param name="deviceResponsePort">The port of the response</param>
        /// <param name="responseMustBeSameInterface">Response must be received at the same interface, on a Docker container, the host receives the package and not the container</param>
        /// <param name="receiveTimeout">The waiting time for a response after the hello package was sent (milliseconds)</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task<DeviceInfoPackage[]> GetDeviceInfoPackagesAsync(
            int deviceListeningPort,
            byte[] deviceHelloPackage,
            int hostSendPort = 0,
            DeviceResponsePort deviceResponsePort = DeviceResponsePort.SendPort,
            bool responseMustBeSameInterface = true,
            int receiveTimeout = 1000,
            CancellationToken cancellationToken = default)
        {
            var items = new List<DeviceInfoPackage>();

            void dataReveived(DeviceInfoPackage package)
            {
                items.Add(package);
            }

            this.DeviceResponseReceived += dataReveived;
            await this.ScanAsync(deviceListeningPort, deviceHelloPackage, hostSendPort, deviceResponsePort, responseMustBeSameInterface, receiveTimeout, cancellationToken).ConfigureAwait(false);
            this.DeviceResponseReceived -= dataReveived;

            return items.ToArray();
        }

        private async Task SendAsync(UdpClient udpClient, int destinationPort, byte[] deviceHelloPackage)
        {
            this._logger.LogDebug($"{nameof(SendAsync)} - Send hello package {BitConverter.ToString(deviceHelloPackage)}");

            var ipEndpoint = new IPEndPoint(IPAddress.Broadcast, destinationPort);
            await udpClient.SendAsync(deviceHelloPackage, deviceHelloPackage.Length, ipEndpoint).ConfigureAwait(false);
        }
    }
}
