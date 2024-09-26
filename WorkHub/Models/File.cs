using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WorkHub.Models
{
    public class File
    {
        [BsonId]
        public ObjectId Id { get; set; }  

        public string TaskID { get; set; }  

        public string FileName { get; set; }  

        public string FilePath { get; set; }  

        public string ContentType { get; set; }  

        public long Size { get; set; }  

        public DateTime UploadedDate { get; set; } = DateTime.Now;  

    }
}