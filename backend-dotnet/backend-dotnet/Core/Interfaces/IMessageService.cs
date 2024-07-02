using backend_dotnet.Core.Dtos.General;
using backend_dotnet.Core.Dtos.Message;
using System.Security.Claims;

namespace backend_dotnet.Core.Interfaces
{
    public interface IMessageService
    {
        Task<GeneralServiceResponseDto> CreateNewMessageAsync(ClaimsPrincipal User, CreateMessageDto createMessageDto);
        Task<IEnumerable<GetMessageDto>> GetMessagesAsync();
        Task<IEnumerable<GetMessageDto>> GetMyMessageAsync(ClaimsPrincipal User);
    }
}
