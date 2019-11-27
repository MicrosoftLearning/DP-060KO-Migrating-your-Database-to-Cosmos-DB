using MongoDB.Driver;
using System;
using System.Configuration;
using System.Net;
using System.Security.Authentication;

namespace MongoDeviceDataCapture
{
    class TemperatureDevice
    {
        private IMongoCollection<ThermometerReading> temperatureCollection;
        private MongoClient client;
        private readonly string deviceName;

        // Retrieve the configuration settings
        private static readonly string address = ConfigurationManager.AppSettings["Address"];
        private static readonly int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
        private static readonly string database = ConfigurationManager.AppSettings["Database"];
        private static readonly string collection = ConfigurationManager.AppSettings["Collection"];
        private static readonly NetworkCredential azureLogin = new NetworkCredential(
            ConfigurationManager.AppSettings["Username"], 
            ConfigurationManager.AppSettings["Password"]);

        public TemperatureDevice(string deviceName)
        {
            this.deviceName = deviceName;

            try
            {
                // Connect to the MongoDB database
                this.client = new MongoClient(new MongoClientSettings
                {
                    Server = new MongoServerAddress(address, port),
                    ServerSelectionTimeout = TimeSpan.FromSeconds(10),

                    //
                    // Credential settings for MongoDB
                    //

                    Credential = MongoCredential.CreateCredential(database, azureLogin.UserName, azureLogin.SecurePassword),

                    //
                    // Credential settings for CosmosDB Mongo API
                    //

                    //UseSsl = true,
                    //SslSettings = new SslSettings
                    //{
                    //    EnabledSslProtocols = SslProtocols.Tls12
                    //},
                    //Credential = new MongoCredential("SCRAM-SHA-1", new MongoInternalIdentity(database, azureLogin.UserName), new PasswordEvidence(azureLogin.SecurePassword))

                    // End of Mongo API settings 
                });

                // Get the collection holding temperature readings
                var db = client.GetDatabase(database);
                var temperatureCollection = db.GetCollection<ThermometerReading>(collection);
                this.temperatureCollection = temperatureCollection;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Device {deviceName} failed with error: {e.Message}");
            }
        }

        // Generate temperature events and write them to the collection in the database
        internal async void RecordTemperatures()
        {
            Random rnd = new Random();

            while (true)
            {
                try
                {
                    // Create a temperature readings
                    ThermometerReading reading = new ThermometerReading
                    {
                        DeviceID = this.deviceName,
                        Temperature = rnd.NextDouble() * 100,
                        Time = DateTime.UtcNow.Ticks
                    };

                    Console.WriteLine($"Recording: {reading.ToString()}");

                    // Write the reading to the database
                    await temperatureCollection.InsertOneAsync(reading);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error recording temperature event: {e.Message}");
                }
            }
        }
    }
}
