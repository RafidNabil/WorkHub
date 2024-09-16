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
        public ObjectId Id { get; set; }  // Unique identifier for the file

        public string TaskID { get; set; }  // ID of the associated task

        public string FileName { get; set; }  // Name of the file

        public string FilePath { get; set; }  // Path where the file is stored

        public string ContentType { get; set; }  // MIME type of the file

        public long Size { get; set; }  // Size of the file in bytes

        public DateTime UploadedDate { get; set; } = DateTime.Now;  // Date when the file was uploaded

    }
}