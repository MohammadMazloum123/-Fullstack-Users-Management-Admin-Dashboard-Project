namespace backend_dotnet.Core.Dtos.Auth
{
    public class LoginServiceResponseDto
    {
        public string NewToken { get; set; }
        //this should be returned to front end
        public UserInfoResult UserInfo { get; set; }
    }
}
