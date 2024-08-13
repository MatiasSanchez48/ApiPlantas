using Core_3.Models;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Cryptography;
using System.Text;

namespace Core_3.Services
{
    public class AuthServices
    {
        private IMongoCollection<Usuario> _usuario;

        public AuthServices(IDBSettings settings)
        {
            var cliente = new MongoClient(settings.Server);
            var database = cliente.GetDatabase("Usuarios");
            _usuario = database.GetCollection<Usuario>("usuarios");
        }
        public async Task<List<Usuario>> GetUsuariosAsync()
        {
            try
            {
                var usuarios = await _usuario.Find(_ => true).ToListAsync();
                return usuarios;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
                throw new Exception("Error fetching users from the database", ex);
            }
        }

        public async Task<Usuario> RegistrarUsuarioAsync(string username, string email, string password)
        {
            if (await _usuario.Find(u => u.Email == email).FirstOrDefaultAsync() != null)
            {
                throw new Exception("Email ya registrado.");
            }

            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);

            var nuevoUsuario = new Usuario
            {
                Username = username,
                Email = email,
                PasswordHash = passwordHash,
                PasswordSalt = passwordSalt,
                IsEmailConfirmed = false,
                PasswordResetToken = null,
                PasswordResetTokenExpiry = DateTime.MinValue
            };

            await _usuario.InsertOneAsync(nuevoUsuario);
            return nuevoUsuario;
        }

        public async Task<Usuario> LoginUsuarioAsync(string email, string password)
        {
            var usuario = await _usuario.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (usuario == null || !VerifyPasswordHash(password, usuario.PasswordHash!, usuario.PasswordSalt!))
            {
                throw new Exception("Credenciales incorrectas.");
            }

            return usuario;
        }

        public async Task SolicitarRecuperacionPasswordAsync(string email, string token, DateTime tokenExpiry)
        {
            var usuario = await _usuario.Find(u => u.Email == email).FirstOrDefaultAsync();
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }

            usuario.PasswordResetToken = token;
            usuario.PasswordResetTokenExpiry = tokenExpiry;

            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, usuario.Id);
            var update = Builders<Usuario>.Update
                .Set(u => u.PasswordResetToken, token)
                .Set(u => u.PasswordResetTokenExpiry, tokenExpiry);

            await _usuario.UpdateOneAsync(filter, update);
        }

        public async Task ResetearPasswordAsync(string token, string newPassword)
        {
            var usuario = await _usuario.Find(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow).FirstOrDefaultAsync();
            if (usuario == null)
            {
                throw new Exception("Token inválido o expirado.");
            }

            CreatePasswordHash(newPassword, out byte[] passwordHash, out byte[] passwordSalt);
            usuario.PasswordHash = passwordHash;
            usuario.PasswordSalt = passwordSalt;
            usuario.PasswordResetToken = null;
            usuario.PasswordResetTokenExpiry = DateTime.MinValue;

            var filter = Builders<Usuario>.Filter.Eq(u => u.Id, usuario.Id);
            var update = Builders<Usuario>.Update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.PasswordSalt, passwordSalt)
                .Set(u => u.PasswordResetToken, null)
                .Set(u => u.PasswordResetTokenExpiry, DateTime.MinValue);

            await _usuario.UpdateOneAsync(filter, update);
        }

        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
            }
        }

        private bool VerifyPasswordHash(string password, byte[] storedHash, byte[] storedSalt)
        {
            using (var hmac = new HMACSHA512(storedSalt))
            {
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != storedHash[i]) return false;
                }
            }
            return true;
        }

        public async Task<Usuario> GetUsuarioByValidTokenAsync(string token)
        {
            var usuario = await _usuario.Find(u => u.PasswordResetToken == token && u.PasswordResetTokenExpiry > DateTime.UtcNow).FirstOrDefaultAsync();
            if (usuario == null)
            {
                throw new Exception("Token inválido o expirado.");
            }
            return usuario;
        }
        public async Task<Usuario> GetUsuarioById(string id)
        {
            var usuario = await _usuario.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (usuario == null)
            {
                throw new Exception("Usuario no encontrado.");
            }
            return usuario;
         
        }
    }
}
