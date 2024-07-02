using System.ComponentModel.DataAnnotations;

namespace backend_dotnet.Core.Dtos.Auth
{
    public class LoginDto
    {
        [Required(ErrorMessage ="User name is required")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Password is required")]
        public string Password { get; set; }
    }
}
