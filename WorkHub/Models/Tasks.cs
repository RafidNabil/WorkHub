using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class Tasks
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        public string TaskTitle { get; set; }

        [DataType(DataType.MultilineText)]
        public string Description { get; set; }

        public string Reporter { get; set; } 

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReporterID { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime AssignDate { get; set; }
        public string Status { get; set; } 
        public List<string> Attachments { get; set; } 
        public double EstimationTime { get; set; } 

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProjectID { get; set; } 
        public List<string> Assignee { get; set; }

    }
}