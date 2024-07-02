using backend_dotnet.Core.Constants;
using backend_dotnet.Core.Dtos.Message;
using backend_dotnet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_dotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MessagesController : ControllerBase
    {
        private readonly IMessageService _messageService;

        public MessagesController(IMessageService messageService)
        {
            _messageService = messageService;
        }

        //Route => create a new message to send to another user
        [HttpPost]
        [Route("create")]
        [Authorize]
        public async Task<IActionResult> CreateNewMessage([FromBody] CreateMessageDto createMessageDto)
        {
            var result = await _messageService.CreateNewMessageAsync(User, createMessageDto);
            if(result.IsSucceed)
                return Ok(result.Message);

            return StatusCode(result.StatusCode, result.Message);
        }

        //Route => Get all messages for current user, either as sender or reciver
        [HttpGet]
        [Route("mine")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<GetMessageDto>>> GetMyMessages()
        {
            var messages = await _messageService.GetMyMessageAsync(User);
            return Ok(messages);
        }

        //Route => Get all messages with Owner and Admin access
        [HttpGet]
        [Authorize(Roles = StaticUserRoles.OwnerAdmin)]
        public async Task<ActionResult<IEnumerable<GetMessageDto>>> GetMessages()
        {
            var messages = await _messageService.GetMessagesAsync();
            return Ok(messages);
        }
    }
}
