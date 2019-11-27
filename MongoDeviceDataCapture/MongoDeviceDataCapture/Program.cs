using System;
using System.Configuration;
using System.Threading;
using System.Threading.Tasks;

namespace MongoDeviceDataCapture
{
    class Program
    {
        private static CancellationTokenSource tokenSource = new CancellationTokenSource();

        static void Main(string[] args)
        {
            int numDevices = int.Parse(ConfigurationManager.AppSettings["NumTemperatureDevices"]);

            try
            {
                // Start recording temperatures
                var driver = new TemperatureDevicesDriver(numDevices);
                var token = tokenSource.Token;
                Task.Factory.StartNew(() => driver.Run(token));
                Task.Factory.StartNew(() => driver.WaitForEnter(tokenSource));
                driver.runCompleteEvent.WaitOne();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Application failed with error: {e.Message}");
            }
        }
    }
}
