using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BrodClientAPI.Models
{
    public class Jobs
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }
        public string ServiceID { get; set; }
        public string UserID { get; set; }
        public string JobStatus { get; set; }

    }
}
