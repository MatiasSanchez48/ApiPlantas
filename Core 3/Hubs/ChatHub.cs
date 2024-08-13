using Core_3.Models;
using Core_3.Services;
using Microsoft.AspNetCore.SignalR;

namespace Core_3.Hubs
{
    public class ChatHub : Hub
    {
        private readonly string _botUser;

        private readonly ChatsServices _chatsServices;
        private readonly IDictionary<string, UserConnection> _conecctions;
        public ChatHub(IDictionary<string, UserConnection> conecctions, ChatsServices chatsServices)
        {
            _botUser = "my chat bot";
            _conecctions = conecctions;
            _chatsServices = chatsServices;
        }

        public async Task<List<ChatMessage>> SendMessage(string message)
        {
            if (_conecctions.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                // Recuperar o crear chat
                var chat = await _chatsServices.GetChatByRoomNameAsync(userConnection.Room);

                var chatMessage = new ChatMessage {
                AutorId = userConnection.User.Id,
                Mensaje = message,
                };

                if (chat != null)
                {

                    await _chatsServices.AddMessageToChat(chat.Id, chatMessage);

                }
                // Notify the group of the new message
                //await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", userConnection.User, chatMessage);

                // Optionally, send the updated message history to the group
                var updatedChat = await _chatsServices.GetChatByRoomNameAsync(userConnection.Room);
                var messages = updatedChat?.Mensajes ?? new List<ChatMessage>();
                //await Clients.Group(userConnection.Room).SendAsync("ReceiveMessageHistory", messages);
                return messages;
            }
            return new List<ChatMessage>();
        }

        public override Task OnDisconnectedAsync(Exception exception)
        {
            if (_conecctions.TryGetValue(Context.ConnectionId, out UserConnection userConnection))
            {
                _conecctions.Remove(Context.ConnectionId);

                Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User} has left");

                SendConnectedUsers(userConnection.Room);
            }
            return base.OnDisconnectedAsync(exception);
        }

        public async Task JoinRoom(UserConnection userConnection, UserConnection userConnected)
        {
            try
        {
                await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);
                _conecctions[Context.ConnectionId] = userConnection;

                // Recuperar o crear chat
                var chat = await _chatsServices.GetChatByRoomNameAsync(userConnection.Room);

                if (chat == null)
                {
                    // Crear un nuevo chat si no existe
                    chat = new Chat
                    {
                        Nombre = userConnection.Room,
                        Participantes = new List<Usuario> { new Usuario { Username = userConnection.User.Username } },
                        Mensajes = new List<ChatMessage>(),
                        FechaCreacion = DateTime.UtcNow
                    };
                    await _chatsServices.CreateChat(userConnection.User.Id, userConnected.User.Id, chat);
                }

                //// Enviar mensajes anteriores al usuario que se une
                var messages = chat.Mensajes ?? new List<ChatMessage>();
                await Clients.Caller.SendAsync("ReceiveMessageHistory", messages);

                //// Notificar a la sala que un nuevo usuario se ha unido
                //await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage", _botUser, $"{userConnection.User.Username} has joined {userConnection.Room}");

                // Enviar lista de usuarios conectados
                await SendConnectedUsers(userConnection.Room);
            }
        catch (Exception ex)
        {
            // Log the error
            Console.WriteLine($"Error in JoinRoom: {ex.Message}");
            throw;
        }
        }

        public Task SendConnectedUsers(string room)
        {
            var users = _conecctions.Values.Where(c => c.Room == room).Select(c => c.User).ToList();
            return Clients.Group(room).SendAsync("UsersInRoom", users);
        }

        //public async Task JoinRoom(UserConnection userConnection)
        //{
        //    await Groups.AddToGroupAsync(Context.ConnectionId, userConnection.Room);

        //    _conecctions[Context.ConnectionId] = userConnection;

        //    await Clients.Group(userConnection.Room).SendAsync("ReceiveMessage",_botUser, $"{userConnection.User} has Joined {userConnection.Room}");

        //    await SendConnectedUsers(userConnection.Room );  
        //}
        //public Task SendConnectedUsers(string room)
        //{
        //    var users = _conecctions.Values.Where(c => c.Room == room).Select(c => c.User).ToList();

        //    return Clients.Group(room).SendAsync("UsersInRoom", users);
        //}
    }
}
