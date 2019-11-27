using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace MongoDeviceDataCapture
{
    // Document structure for capturing and storing temperature readings
    public class ThermometerReading
    {
        [BsonId]
        public ObjectId ID { get; set; }

        [BsonElement("deviceID")]
        public string DeviceID { get; set; }

        [BsonElement("temperature")]
        public double Temperature { get; set; }

        [BsonElement("time")]
        public long Time { get; set; }

        public override string ToString()
        {
            return $"DeviceID: {DeviceID}, Temperature: {Temperature} Time: {Time}";
        }
    }
}
