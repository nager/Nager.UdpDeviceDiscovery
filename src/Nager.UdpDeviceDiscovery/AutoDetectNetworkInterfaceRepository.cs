using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Nager.UdpDeviceDiscovery
{
    public class AutoDetectNetworkInterfaceRepository : INetworkInterfaceRepository
    {
        /// <inheritdoc/>
        public IList<NetworkInterface> GetNetworkInterfaces()
        {
            return this.GetAvailableNetworkInterfaces(NetworkInterfaceType.Ethernet);
        }

        private NetworkInterface[] GetAvailableNetworkInterfaces(NetworkInterfaceType networkInterfaceType)
        {
            var networkInfos = new List<NetworkInterface>();
            foreach (var networkInterface in System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces())
            {
                if (networkInterface.NetworkInterfaceType == networkInterfaceType && networkInterface.OperationalStatus == OperationalStatus.Up)
                {
                    foreach (var ip in networkInterface.GetIPProperties().UnicastAddresses)
                    {
                        if (ip.Address.AddressFamily == AddressFamily.InterNetwork)
                        {
                            networkInfos.Add(new NetworkInterface
                            {
                                IpAddress = ip.Address.ToString(),
                                SubnetMask = ip.IPv4Mask.ToString()
                            });
                        }
                    }
                }
            }
            return networkInfos.ToArray();
        }
    }
}
