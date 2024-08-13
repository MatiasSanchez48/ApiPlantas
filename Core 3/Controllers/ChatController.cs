using Core_3.Models;
using Core_3.Services;
using Microsoft.AspNetCore.Mvc;

namespace Core_3.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ChatController : ControllerBase
    {
        private readonly ChatsServices _chatsServices;

        public ChatController(ChatsServices chatsServices)
        {
            _chatsServices = chatsServices;
        }

        [HttpGet]
        public async IAsyncEnumerable<List<Chat>> GetChatMessages()
        {
            yield return await _chatsServices.GetChats();
        }


        [HttpGet("{id}")]
        public async IAsyncEnumerable<Chat> GetMessageById(string id)
        {
            var message = await _chatsServices.GetChatById(id);

         yield   return message;
        }

        [HttpPost("{autorId}/{messageSendId}")]
        public async IAsyncEnumerable<Chat> CreateMessage(string autorId,string messageSendId,[FromBody] Chat message)
        {

           var newChat = await _chatsServices.CreateChat(autorId, messageSendId, message);

          yield  return newChat;
        }

        [HttpPost("{chatId}")]
        public async IAsyncEnumerable<ChatMessage> AddMessageToChat(string chatId,[FromBody] ChatMessage message)
        {

         var newMessage =   await _chatsServices.AddMessageToChat(chatId,message);

         yield   return newMessage;
        }

        [HttpPut("{chatId}")]
        public async IAsyncEnumerable<ActionResult> MarkMessageAsRead(string chatId, [FromQuery]  string messageId, [FromQuery] string userId)
        {
          await _chatsServices.MarkMessageAsRead(chatId,messageId,userId);

            yield return Ok();
        }

        [HttpPut]
        public async IAsyncEnumerable<ActionResult> UpdateMessage(string chatId, [FromBody] ChatMessage message)
        {
            if(message.Id != null) await _chatsServices.UpdateMessageInChat(chatId,message.Id,message);

            yield return Ok();
        }

        [HttpDelete("message/{chatId}")]
        public async IAsyncEnumerable<ActionResult> DeleteMessageFromChat(string chatId, [FromQuery] string messageId)
        {
           await _chatsServices.DeleteMessageFromChat(chatId, messageId);

            yield return Ok();
        }
        [HttpDelete("{chatId}")]
        public async IAsyncEnumerable<ActionResult> DeleteMessageFromChat(string chatId)
        {
            await _chatsServices.DeleteAllMessagesFromChat(chatId);

            yield return Ok();
        }
    }

}
