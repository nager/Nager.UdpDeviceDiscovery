# Nager.UdpDeviceDiscovery

A lightweight and flexible **UDP device discovery library** for .NET. This project allows you to scan local networks for devices that respond to a predefined "hello" message and collect their responses asynchronously. It's designed to handle multiple network interfaces, Docker environments, and custom response ports efficiently.  

## Features

- Broadcast a custom UDP "hello" message to all network interfaces  
- Receive and handle device responses asynchronously  
- Support for Docker or multi-interface environments  
- Configurable receive timeout and response port options  
- Easy integration with logging (`Microsoft.Extensions.Logging`)  
- Retrieve detailed device info including IP address and raw response data  

## Installation
The package is available on [NuGet](https://www.nuget.org/packages/Nager.UdpDeviceDiscovery)
```
PM> install-package Nager.UdpDeviceDiscovery
```

## Usage Example

### MOXA NPort discovery

```cs
var helloPackage = new byte[] { 0x01, 0x00, 0x00, 0x08, 0x00, 0x00, 0x00, 0x00 };
var deviceListeningPort = 4800;

var deviceDetector = new UdpDeviceDetector();
var packages = await deviceDetector.GetDeviceInfoPackagesAsync(deviceListeningPort, helloPackage, receiveTimeout: 2000);
```
