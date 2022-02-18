# Nager.UdpDeviceDiscovery
Discover devices on the network via an udp broadcast

## nuget
The package is available on [nuget](https://www.nuget.org/packages/Nager.UdpDeviceDiscovery)
```
PM> install-package Nager.UdpDeviceDiscovery
```

## Examples

### MOXA NPort discovery

```cs
var helloPackage = new byte[] { 0x01, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };

var deviceDetector = new UdpDeviceDetector();
var packages = await deviceDetector.GetDeviceInfoPackagesAsync(4800, helloPackage, receiveTimeout: 2000);
```
