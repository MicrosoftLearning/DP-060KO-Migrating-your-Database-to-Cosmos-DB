using System;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDeviceDataCapture
{
    public class TemperatureDevicesDriver
    {
        public ManualResetEvent runCompleteEvent = new ManualResetEvent(false);
        private int numDevices;

        public TemperatureDevicesDriver(int numDevices)
        {
            this.numDevices = numDevices;
        }

        public void Run(CancellationToken token)
        {
            var rnd = new Random();

            for (int deviceNum = 0; deviceNum < this.numDevices; deviceNum++)
            {
                string deviceName = $"Device {deviceNum}";

                var temperatureDevice = new TemperatureDevice(deviceName);
                Task.Factory.StartNew(() => temperatureDevice.RecordTemperatures());
            }

            while (!token.IsCancellationRequested)
            {
                // Run until the user stops the devices by pressing Enter
            }

            this.runCompleteEvent.Set();
        }

        public void WaitForEnter(CancellationTokenSource tokenSource)
        {
            Console.WriteLine("Press Enter to stop devices");
            Console.ReadLine();
            tokenSource.Cancel();
        }
    }
}
