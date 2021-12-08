namespace Nager.UdpDeviceDiscovery
{
    /// <summary>
    /// Port to which the response of the device is sent
    /// </summary>
    public enum DeviceResponsePort
    {
        /// <summary>
        /// The port of the package is sent (Default)
        /// </summary>
        SendPort = 0,
        /// <summary>
        /// The port the device is listening
        /// </summary>
        ListeningPort = 1
    }
}
