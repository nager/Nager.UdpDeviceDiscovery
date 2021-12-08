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

            var networkInterfaceRepository = new StaticNetworkInterfaceRepository("192.168.0.248", "255.255.255.0");
            var deviceDetector = new UdpDeviceDetector(logger, networkInterfaceRepository);

            var cancellationTokenSource = new CancellationTokenSource();
            cancellationTokenSource.CancelAfter(2000);

            var helloPackage = Encoding.UTF8.GetBytes("$BRD,#");

            deviceDetector.DeviceResponseReceived += DeviceResponseReceived;
            var packages = await deviceDetector.GetDeviceInfoPackagesAsync(4800, helloPackage, receiveTimeout: 1000);
            await deviceDetector.ScanAsync(65535, helloPackage, 0, DeviceResponsePort.ListeningPort, true, 1000, cancellationTokenSource.Token);
            deviceDetector.DeviceResponseReceived -= DeviceResponseReceived;
        }

        private static void DeviceResponseReceived(DeviceInfoPackage deviceInfoPackage)
        {
            Console.WriteLine(deviceInfoPackage.DeviceIpAddress, deviceInfoPackage.DiscoveredNetwork);
        }
    }
}
