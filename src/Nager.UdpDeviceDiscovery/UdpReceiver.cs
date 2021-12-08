using Microsoft.Extensions.Logging;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Nager.UdpDeviceDiscovery
{
    public class UdpReceiver : IDisposable
    {
        private readonly ILogger _logger;
        private readonly UdpClient _udpClient;
        private bool _disposed;

        public event Action<byte[], IPAddress> DataReceived;

        public UdpReceiver(
            ILogger logger,
            IPEndPoint ipEndPoint)
        {
            this._logger = logger;

            this._udpClient = new UdpClient
            {
                ExclusiveAddressUse = false
            };

            this._udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            this._udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.Broadcast, true);

            this._udpClient.Client.Bind(ipEndPoint);
        }

        public void Dispose()
        {
            this.Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            this._disposed = true;
            this._udpClient?.Close();
            this._udpClient?.Dispose();
        }

        public async Task ReceiveAsync()
        {
            this._logger.LogDebug($"{nameof(ReceiveAsync)} - Start receive process...");

            try
            {
                this._logger.LogDebug($"{nameof(ReceiveAsync)} - Wait for data...");
                while (!this._disposed)
                {
                    var result = await this._udpClient.ReceiveAsync().ConfigureAwait(false);

                    this._logger.LogInformation($"{nameof(ReceiveAsync)} - Data received from:{result.RemoteEndPoint.Address}, data:{BitConverter.ToString(result.Buffer)}");
                    this.DataReceived?.Invoke(result.Buffer, result.RemoteEndPoint.Address);
                }
            }
            catch (ObjectDisposedException)
            {
                //No Log required
            }
            catch (Exception exception)
            {
                this._logger.LogError(exception, $"{nameof(ReceiveAsync)}");
            }
            finally
            {
                this._logger.LogDebug($"{nameof(ReceiveAsync)} - Stop receive process");
            }
        }
    }
}
