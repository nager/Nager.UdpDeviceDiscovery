using System.Collections.Generic;

namespace Nager.UdpDeviceDiscovery
{
    /// <summary>
    /// Interface NetworkInterface Repository
    /// </summary>
    public interface INetworkInterfaceRepository
    {
        /// <summary>
        /// Returns the network interfaces to be scanned
        /// </summary>
        /// <returns></returns>
        IList<NetworkInterface> GetNetworkInterfaces();
    }
}
