using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace notes_backend.Domain.Entities
{
    public class Note
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = ObjectId.GenerateNewId().ToString();

        [BsonElement("patientId")]
        public int PatientId { get; set; }

        [BsonElement("doctorName")]
        public string DoctorName { get; set; } = string.Empty;

        [BsonElement("noteContent")]
        public string NoteContent { get; set; } = string.Empty;

        [BsonElement("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updatedAt")]
        public DateTime? UpdatedAt { get; set; }

    }
}
