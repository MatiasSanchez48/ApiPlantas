using Microsoft.AspNetCore.SignalR;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
namespace Core_3.Models

{
    public class ChatMessage
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("autorId")]
        public string? AutorId { get; set; }

        [BsonElement("mensaje")]
        public string? Mensaje { get; set; }

        [BsonElement("urlImagen")]
        public string? UrlImagen { get; set; }

        [BsonElement("fechaCreacion")]
        public DateTime? FechaCreacion { get; set; }

        [BsonElement("fechaModificacion")]
        public DateTime? FechaModificacion { get; set; }

        [BsonElement("fechaEliminacion")]
        public DateTime? FechaEliminacion { get; set; }

        [BsonElement("leidoPor")]
        public List<MessageReadingStatus>? LeidoPor { get; set; }
    }

    public class Chat 
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("nombre")]
        public string? Nombre { get; set; }

        [BsonElement("participantes")]
        public List<Usuario>? Participantes { get; set; }

        [BsonElement("mensajes")]
        public List<ChatMessage>? Mensajes { get; set; }

        [BsonElement("fechaCreacion")]
        public DateTime FechaCreacion { get; set; }

        [BsonElement("fechaEliminacion")]
        public DateTime? FechaEliminacion { get; set; }

    }
    public class MessageReadingStatus
    {
        public string? UserId { get; set; }
        public string? MessageId { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadTime { get; set; }
    }
}
