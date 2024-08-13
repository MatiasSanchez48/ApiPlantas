using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson;

namespace Core_3.Models
{
    public class Usuario
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
         public string? Id { get; set; }

        [BsonElement("username")]
        public string? Username { get; set; }

        [BsonElement("email")]
        public string? Email { get; set; }

        [BsonElement("passwordHash")]
        public byte[]? PasswordHash { get; set; }

        [BsonElement("passwordSalt")]
        public byte[]? PasswordSalt { get; set; }

        [BsonElement("isEmailConfirmed")]
        public bool IsEmailConfirmed { get; set; }

        [BsonElement("passwordResetToken")]
        public string? PasswordResetToken { get; set; }

        [BsonElement("passwordResetTokenExpiry")]
        public DateTime PasswordResetTokenExpiry { get; set; }

        [BsonElement("urlImage")]
        public string? UrlImage { get; set; }
        
    }

    public class RegistrarUsuarioDto
    {
       required public string Username { get; set; }
        required public string Email { get; set; }
        required public string Password { get; set; }
    }

    public class LoginUsuarioDto
    {
        required public string Email { get; set; }
        required public string Password { get; set; }
    }

    public class SolicitarRecuperacionDto
    {
        required public string Email { get; set; }
    }

    public class ResetearPasswordDto
    {
        required public string Token { get; set; }
        required public string NewPassword { get; set; }
    }
}
