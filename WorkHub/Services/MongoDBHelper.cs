using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace WorkHub.Services
{
    public static class MongoDBHelper
    {
        private static IMongoClient _client;
        private static IMongoDatabase _database;

        static MongoDBHelper()
        {
            string connectionString = ConfigurationManager.ConnectionStrings["MongoDB"].ConnectionString;

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new ArgumentNullException("MongoDB connection string is missing or empty.");
            }

            _client = new MongoClient(connectionString);

            var mongoUrl = new MongoUrl(connectionString);
            _database = _client.GetDatabase(mongoUrl.DatabaseName);

            if (_database == null)
            {
                throw new ArgumentNullException("MongoDB database is null.");
            }
        }

        public static IMongoCollection<T> GetCollection<T>(string collectionName)
        {
            return _database.GetCollection<T>(collectionName);
        }
    }
}