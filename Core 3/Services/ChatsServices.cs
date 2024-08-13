using Core_3.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System;
using System.Linq;
using ZstdSharp;

namespace Core_3.Services
{
    public class ChatsServices
    {
        private IMongoCollection<Chat> _chats;
        private AuthServices _authServices;

        public ChatsServices(IDBSettings settings, AuthServices authServices)
        {
            var cliente = new MongoClient(settings.Server);
            var database = cliente.GetDatabase(settings.Database);
            _chats = database.GetCollection<Chat>(settings.Collection);
            _authServices = authServices;
        }
        
        public async Task<List<Chat>> GetChats()
        {
            var filter = Builders<Chat>.Filter.Eq(chat => chat.FechaEliminacion, null);

            var chats = await _chats.Find(filter).ToListAsync();

            return chats;
        }

        public async Task<Chat>GetChatById(string id)
        {
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, id);
            return await _chats.Find(filter).FirstOrDefaultAsync();
        }

        public async Task<Chat> CreateChat(string autorId,string messageSendId,Chat chat)
        {

            var autorTask =   _authServices.GetUsuarioById(autorId);
            var receptorTask =  _authServices.GetUsuarioById(messageSendId);

            Task.WaitAll(autorTask, receptorTask); // Esperar a que las tareas se completen

            var autor = autorTask.Result;
            var receptor = receptorTask.Result;
            // Verificar y agregar participantes si no están ya en la lista
            if (chat.Participantes == null)
            {
                chat.Participantes = new List<Usuario>();
            } 
            if (!chat.Participantes.Contains(autor))
            {
                chat.Participantes.Add(autor);
            }
            if (!chat.Participantes.Contains(receptor))
            {
                chat.Participantes.Add(receptor);
            }
          
            chat.FechaCreacion = DateTime.UtcNow;

           await _chats.InsertOneAsync(chat);

           var newMensagge = await AddMessageToChat(chat.Id!,chat.Mensajes!.First());

            if (chat.Mensajes != null)
            {
                foreach (var mensaje in chat.Mensajes)
                {
                    
                    if (newMensagge.LeidoPor == null)
                    {
                        newMensagge.LeidoPor = new List<MessageReadingStatus>();
                    }
                    if (!newMensagge.LeidoPor.Any(d=>  d.UserId == autor.Id))
                    {
                        var messageRead = new MessageReadingStatus()
                        {
                            IsRead = true,
                            UserId = autorId,
                            MessageId = mensaje.Id,
                            ReadTime = DateTime.UtcNow,
                        };

                        newMensagge.LeidoPor?.Add(messageRead);
                    }
                   
                }
            }

            return chat;
        }

        public async Task<ChatMessage> AddMessageToChat(string chatId, ChatMessage message)
        {
            message.Id = ObjectId.GenerateNewId().ToString();
            message.FechaCreacion = DateTime.UtcNow; 

            if (message.AutorId == null) throw new Exception("autor no encontrado");

            var messageRead = new MessageReadingStatus()
            {
                IsRead = true,
                UserId = message.AutorId,
                MessageId = message.Id.ToString(),
                ReadTime = DateTime.UtcNow,
            };
            if (message.LeidoPor == null)
            {
                message.LeidoPor = new List<MessageReadingStatus>();
            }

            message.LeidoPor.Add(messageRead);

            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            var update = Builders<Chat>.Update.Push(c => c.Mensajes, message);
            await _chats.UpdateOneAsync(filter, update);

            return message;
        }
        
        public async Task UpdateChat(Chat chat)
        {
            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chat.Id);
            await _chats.ReplaceOneAsync(filter, chat);
        }

        public async Task MarkMessageAsRead(string chatId, string messageId, string userId)
        {
            var chat = await GetChatById(chatId);
            if (chat == null) throw new Exception("Chat no encontrado");

            var usuario = _authServices.GetUsuarioById(userId);
            if (usuario == null) throw new Exception("Usuario no encontrado");
       
            var message =  chat.Mensajes?.Find(m => m.Id == messageId);
            if (message == null) throw new Exception("Mensaje no encontrado");

            // Verificar si el usuario ya ha marcado como leído el mensaje
            var usuarioEnLista = message.LeidoPor?.Any(u => u.UserId == usuario.Result.Id);


            if (!usuarioEnLista ?? false)
            {
                var messageRead = new MessageReadingStatus()
                {
                    IsRead = true,
                    UserId = usuario.Result.Id ,
                    MessageId = message.Id,
                    ReadTime = DateTime.Now,
                };

                message.LeidoPor.Add(messageRead);
                message.FechaModificacion = DateTime.UtcNow;
            }

            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            await _chats.ReplaceOneAsync(filter, chat);
        }

        public async Task DeleteMessageFromChat(string chatId, string messageId)
        {
            var chat = await GetChatById(chatId);
            if (chat == null) throw new Exception("Chat no encontrado");

            var message = chat.Mensajes.Find(m => m.Id == messageId);
            if (message == null) throw new Exception("Mensaje no encontrado");
            if (message.FechaEliminacion.HasValue) throw new Exception("El mensaje ya está eliminado");

            message.FechaEliminacion = DateTime.UtcNow;

            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            await _chats.ReplaceOneAsync(filter, chat);
        }

        public async Task DeleteAllMessagesFromChat(string chatId)
        {
            var chat = await GetChatById(chatId);
            if (chat == null) throw new Exception("Chat no encontrado");

            foreach (var message in chat.Mensajes?? [])
            {
                if (!message.FechaEliminacion.HasValue)
                {
                    message.FechaEliminacion = DateTime.UtcNow;
                }
            }

            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            await _chats.ReplaceOneAsync(filter, chat);
        }

        public async Task UpdateMessageInChat(string chatId, string messageId, ChatMessage updatedMessage)
        {
            var chat = await GetChatById(chatId);
            if (chat == null) throw new Exception("Chat no encontrado");

            var message = chat.Mensajes?.Find(m => m.Id == messageId);
            if (message == null) throw new Exception("Mensaje no encontrado");

            if (!string.IsNullOrEmpty(updatedMessage.Mensaje)) message.Mensaje = updatedMessage.Mensaje;
            if (!string.IsNullOrEmpty(updatedMessage.UrlImagen)) message.UrlImagen = updatedMessage.UrlImagen;
            message.FechaModificacion = DateTime.UtcNow;

            var filter = Builders<Chat>.Filter.Eq(c => c.Id, chatId);
            await _chats.ReplaceOneAsync(filter, chat);
        }
        public async Task<Chat?> GetChatByRoomNameAsync(string roomName)
        {
            var filter = Builders<Chat>.Filter.Eq(chat => chat.Nombre, roomName);
            return await _chats.Find(filter).FirstOrDefaultAsync();
        }

    }
}
