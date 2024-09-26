using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class TaskCompletionRequest
    {
        [BsonId]
        public ObjectId RequestId { get; set; }

        [BsonElement("TaskId")]
        public string TaskId { get; set; }

        [BsonElement("DeveloperId")]
        public string DeveloperId { get; set; }

        [BsonElement("ManagerId")]
        public string ManagerId { get; set; }

        [BsonElement("RequestDate")]
        public DateTime RequestDate { get; set; }

        [BsonElement("Comment")]
        public string Comment { get; set; }

        [BsonElement("Status")]
        public string Status { get; set; } 

        [BsonElement("ReviewDate")]
        public DateTime? ReviewDate { get; set; }

    }
}