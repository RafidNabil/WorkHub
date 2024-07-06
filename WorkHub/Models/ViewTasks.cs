using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class ViewTasks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public ObjectId Id { get; set; }

        public string TaskName { get; set; }
        public string Description { get; set; }
        public string Status { get; set; }
        public string Assignee { get; set; }
        public DateTime DueDate { get; set; }
    }
}