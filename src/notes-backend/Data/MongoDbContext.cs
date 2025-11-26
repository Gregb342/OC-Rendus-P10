using Microsoft.Extensions.Options;
using MongoDB.Driver;
using notes_backend.Domain.Entities;
using notes_backend.Infrastructure.Settings;

namespace notes_backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            var client = new MongoClient(settings.Value.ConnectionString);
            _database = client.GetDatabase(settings.Value.DatabaseName);
        }

        public IMongoCollection<Note> Notes =>
            _database.GetCollection<Note>(GetCollectionName());

        private string GetCollectionName()
        {
            var settings = _database.Client.Settings;
            return "Notes";
        }
    
    }
}
