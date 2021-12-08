namespace Nager.UdpDeviceDiscovery
{
    public class DeviceInfoPackage
    {
        public NetworkInterface DiscoveredNetwork { get; set; }
        public string DeviceIpAddress { get; set; }
        public byte[] ReceivedData { get; set; }
    }
}
