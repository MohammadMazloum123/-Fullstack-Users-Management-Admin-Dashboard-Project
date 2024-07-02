namespace backend_dotnet.Core.Dtos.Message
{
    public class CreateMessageDto
    {
        public string ReceiverUserName { get; set; }
        public string Text { get; set; }
    }
}
