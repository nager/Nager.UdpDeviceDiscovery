using Microsoft.Extensions.Logging;
using System;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Nager.UdpDeviceDiscovery.TestConsole
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.SetMinimumLevel(LogLevel.Debug);
                builder.AddConsole();
            });

            var logger = loggerFactory.CreateLogger<UdpDeviceDetector>();

            await Scan(logger);
            //await ScanViaDefinedNetworkInterface(logger);
            //await ScanWithEvents(logger);
        }

        static async Task Scan(ILogger<UdpDeviceDetector> logger)
        {
            var helloPackage = new byte[] { 0x02, 0x35, 0x38, 0x2E, 0x30, 0x03, 0x10 };

            var deviceDetector = new UdpDeviceDetector(logger);
            var packages = await deviceDetector.GetDeviceInfoPackagesAsync(12000, helloPackage, receiveTimeout: 1000);
        }

        static async Task ScanViaDefinedNetworkInterface(ILogger<UdpDeviceDetector> logger)
        {
            var helloPackage = new byte[] { 0x02, 0x35, 0x38, 0x2E, 0x30, 0x03, 0x10 };

            var networkInterfaceRepository = new StaticNetworkInterfaceRepository("192.168.0.248", "255.255.255.0");
            var deviceDetector = new UdpDeviceDetector(logger, networkInterfaceRepository);
            var packages = await deviceDetector.GetDeviceInfoPackagesAsync(12000, helloPackage, receiveTimeout: 1000);
        }

        static async Task ScanWithEvents(ILogger<UdpDeviceDetector> logger)
        {
            var helloPackage = Encoding.UTF8.GetBytes("$BRD,#");

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(2000);

            var deviceDetector = new UdpDeviceDetector(logger);
            deviceDetector.DeviceResponseReceived += DeviceResponseReceived;
            await deviceDetector.ScanAsync(65535, helloPackage, 0, DeviceResponsePort.ListeningPort, true, 1000, cancellationTokenSource.Token);
            deviceDetector.DeviceResponseReceived -= DeviceResponseReceived;
        }

        private static void DeviceResponseReceived(DeviceInfoPackage deviceInfoPackage)
        {
            Console.WriteLine(deviceInfoPackage.DeviceIpAddress, deviceInfoPackage.DiscoveredNetwork);
        }
    }
}
