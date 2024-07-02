using System.ComponentModel.DataAnnotations;

namespace backend_dotnet.Core.Dtos.Auth
{
    public class UpdateRoleDto
    {
        [Required(ErrorMessage = "User name is required~")]
        public string UserName { get; set; }

        public RoleType NewRole {  get; set; }
    }

    public enum RoleType 
    {
        ADMIN,
        MANAGER,
        USER
    }
}
