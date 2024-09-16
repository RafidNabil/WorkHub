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

        public string Reporter { get; set; } // This will be a dropdown/list in the view

        [BsonRepresentation(BsonType.ObjectId)]
        public string ReporterID { get; set; }

        [DataType(DataType.Date)]
        public DateTime DueDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime AssignDate { get; set; }
        public string Status { get; set; } // Dropdown with options: To-Do, In Progress, Done
        public List<string> Attachments { get; set; } // File upload paths
        public double EstimationTime { get; set; } // Number input

        [BsonRepresentation(BsonType.ObjectId)]
        public string ProjectID { get; set; } // Foreign key to the project
        public List<string> Assignee { get; set; }

    }
}