using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class UserNotification
    {
        [BsonId]
        public ObjectId NotificationId { get; set; }

        [BsonElement("UserId")]
        public string UserId { get; set; }

        [BsonElement("NotificationDate")]
        public DateTime NotificationDate { get; set; }

        [BsonElement("Message")]
        public string Message { get; set; }

        [BsonElement("IsRead")]
        public bool IsRead { get; set; }

        [BsonElement("RedirectUrl")]
        public string RedirectUrl { get; set; } // URL or route information for navigation

    }
}