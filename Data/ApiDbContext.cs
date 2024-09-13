﻿using BrodClientAPI.Models;
using MongoDB.Driver;
using MongoDB.Bson;


namespace BrodClientAPI.Data
{
    public class ApiDbContext
    {
        private readonly IMongoDatabase _database;

        public ApiDbContext(IMongoClient client)
        {
            try
            {
                _database = client.GetDatabase("BrodClientDB");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error connecting to MongoDB: {ex.Message}");
                throw;
            }
        }

        public IMongoCollection<User> User => _database.GetCollection<User>("User");
        public IMongoCollection<Services> Services => _database.GetCollection<Services>("Services");

        public void Initialize()
        {
            try
            {
                // Create an index on the Username field
                User.Indexes.CreateOne(new CreateIndexModel<User>(Builders<User>.IndexKeys.Ascending(u => u._id)));
                Services.Indexes.CreateOne(new CreateIndexModel<Services>(Builders<Services>.IndexKeys.Ascending(u => u._id)));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating indexes: {ex.Message}");
                throw;
            }
        }
    }
}
