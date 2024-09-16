using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class KanbanCard
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }
        public string ColumnNo { get; set; }
        public string Header { get; set; }
        public string Body { get; set; }
        public string ImageUrl { get; set; }
        public int CommentsCount { get; set; }
        public int AttachmentsCount { get; set; }
        public string AvatarUrl { get; set; }
    }
}