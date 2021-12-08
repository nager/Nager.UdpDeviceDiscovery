using System.Collections.Generic;

namespace Nager.UdpDeviceDiscovery
{
    /// <summary>
    /// Static NetworkInterface Repository
    /// </summary>
    public class StaticNetworkInterfaceRepository : INetworkInterfaceRepository
    {
        private readonly NetworkInterface _networkInterface;

        public StaticNetworkInterfaceRepository(string ipAddress, string subnetMask)
        {
            this._networkInterface = new NetworkInterface
            {
                IpAddress = ipAddress,
                SubnetMask = subnetMask
            };
        }

        /// <inheritdoc/>
        public IList<NetworkInterface> GetNetworkInterfaces()
        {
            return new NetworkInterface[] { this._networkInterface };
        }
    }
}
