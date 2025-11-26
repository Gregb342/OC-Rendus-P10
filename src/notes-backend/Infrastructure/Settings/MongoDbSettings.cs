namespace notes_backend.Infrastructure.Settings
{
    public class MongoDbSettings
    {
        public string ConnectionString { get; set; } = string.Empty;
        public string DatabaseName { get; set;} = string.Empty;
        public string NotesCollectionName { get; set;} = string.Empty;
    }
}
