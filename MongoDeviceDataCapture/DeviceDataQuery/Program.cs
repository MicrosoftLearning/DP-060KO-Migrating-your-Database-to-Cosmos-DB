using System;
using System.Configuration;
using MongoDB.Driver;
using System.Net;
using MongoDB.Bson;
using System.Security.Authentication;

namespace DeviceDataQuery
{
    class Program
    {
        private static MongoClient client;

        // Retrieve the configuration settings
        private static readonly string address = ConfigurationManager.AppSettings["Address"];
        private static readonly int port = int.Parse(ConfigurationManager.AppSettings["Port"]);
        private static readonly string database = ConfigurationManager.AppSettings["Database"];
        private static readonly string collection = ConfigurationManager.AppSettings["Collection"];
        private static readonly NetworkCredential azureLogin = new NetworkCredential(
            ConfigurationManager.AppSettings["Username"],
            ConfigurationManager.AppSettings["Password"]);

        static void Main(string[] args)
        {
            string input = "";
            int deviceNum = 0;

            client = ConnectToDatabase();
            if (client != null)
            {
                var db = client.GetDatabase(database);
                var temperatureCollection = db.GetCollection<ThermometerReading>(collection);

                var statsQuery = new BsonDocument
                {
                    {
                        "_id",  $"Device {deviceNum}"
                    },
                    {
                        "NumReadings", new BsonDocument {{"$sum", 1}}
                    },
                    {
                        "AverageTemperature", new BsonDocument {{"$avg", "$temperature"}}
                    },
                    {
                        "LowestReading", new BsonDocument {{"$min", "$temperature"}}
                    },
                    {
                        "HighestReading", new BsonDocument {{"$max", "$temperature"}}
                    },
                    {
                        "LatestReading", new BsonDocument {{"$last", "$temperature"}}
                    }
                };
                    

                while (!string.Equals(input, "Q") && !string.Equals(input, "q"))
                {
                    Console.WriteLine("Enter Device Number ('Q' to quit)");
                    input = Console.ReadLine();
                    if (int.TryParse(input, out deviceNum))
                    {
                        // Fetch the stats for the specified device and display them
                        var match = new BsonDocument
                        {
                            {"deviceID", $"Device {deviceNum}" }
                        };

                        var stats = temperatureCollection.Aggregate().Match(match).Group(statsQuery);
                        var data = stats.ToList();
                        foreach (var results in data)
                        {
                            Console.WriteLine($"Device: {deviceNum}, Readings: {results["NumReadings"]}, Lowest: {results["LowestReading"]}, Highest: {results["HighestReading"]}, Average: {results["AverageTemperature"]}, Latest: {results["LatestReading"]}");
                        }
                    }
                }
            }
        }

        private static MongoClient ConnectToDatabase()
        {
            try
            {
                // Connect to the MongoDB database
                MongoClient client = new MongoClient(new MongoClientSettings
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

                return client;
            }
            catch (Exception e)
            {
                Console.WriteLine($"Failed to connect to database: {e.Message}");
                return null;
            }
        }
    }
}
