using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class Notification
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string UserId { get; set; } 

        public string Message { get; set; } 

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow; 

        public bool IsRead { get; set; } = false; 
    }
}