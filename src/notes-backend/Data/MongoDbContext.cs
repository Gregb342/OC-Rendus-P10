using Microsoft.Extensions.Options;
using MongoDB.Driver;
using notes_backend.Domain.Entities;
using notes_backend.Infrastructure.Settings;

namespace notes_backend.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;
        private readonly MongoDbSettings _settings;

        public MongoDbContext(IOptions<MongoDbSettings> settings)
        {
            _settings = settings.Value;
            var client = new MongoClient(_settings.ConnectionString);
            _database = client.GetDatabase(_settings.DatabaseName);
        }

        public IMongoCollection<Note> Notes =>
            _database.GetCollection<Note>(_settings.NotesCollectionName);
    }
}
