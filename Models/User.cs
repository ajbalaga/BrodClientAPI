﻿using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace BrodClientAPI.Models
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string _id { get; set; }  // ObjectId in MongoDB
        public string Username { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Role { get; set; } // "Admin", "Client", "Tradie"
        public string BusinessPostCode { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string ContactNumber { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string ProximityToWork { get; set; }
        public string RegisteredBusinessName { get; set; }
        public string AustralianBusinessNumber { get; set; }
        public string TypeofWork { get; set; }
        public string Status { get; set; }
        public string ReasonforDeclinedApplication { get; set; }
        public string AboutMeDescription { get; set; }
        public string Website { get; set; }
        public string FacebookAccount { get; set; }
        public string IGAccount { get; set; }
        public List<string> Services { get; set; }
        public string ProfilePicture { get; set; } // Base64 string for profile picture
        public List<CertificationFile> CertificationFilesUploaded { get; set; } // List of Base64 strings for certification files
        public string AvailabilityToWork { get; set; }
        public int ActiveJobs { get; set; } = 0;
        public int PendingOffers { get; set; } = 0;
        public int CompletedJobs { get; set; } = 0;
        public decimal EstimatedEarnings { get; set; }= 0;
        public string CallOutRate { get; set; }
        public int PublishedAds { get; set; } = 0;

    }

    public class CertificationFile
    {
        public string FileName { get; set; }
        public string Base64 { get; set; }
    }

}
