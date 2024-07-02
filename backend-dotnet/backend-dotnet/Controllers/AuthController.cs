using backend_dotnet.Core.Constants;
using backend_dotnet.Core.Dtos.Auth;
using backend_dotnet.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace backend_dotnet.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //Route => Seed roles to DB
        [HttpPost]
        [Route("seed-roles")]
        public async Task<IActionResult> SeedRoles()
        {
            var seedResult = await _authService.SeedRoleAsync();
            return StatusCode(seedResult.StatusCode, seedResult.Message);
        }

        //Route => Register
        [HttpPost]
        [Route("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto registerDto)
        {
            var registeResult = await _authService.RegisterAsync(registerDto);
            return StatusCode(registeResult.StatusCode, registeResult.Message);
        }

        //Route => Login
        [HttpPost]
        [Route("login")]
        public async Task<ActionResult<LoginServiceResponseDto>> Login([FromBody] LoginDto loginDto)
        {
            var loginResult = await _authService.LoginAsync(loginDto);
            if (loginResult is null)
            {
                return Unauthorized("Your Credentials are invalid, Please Contact an Admin");
            }
            return Ok(loginResult);
        }

        //Route => Update User Role
        //Owner can change everything
        //Admin can change user to manager or vice versa
        //User and Manager Roles dont have access
        [HttpPost]
        [Route("update-role")]
        [Authorize(Roles = StaticUserRoles.OwnerAdmin)]
        public async Task<IActionResult> Update([FromBody] UpdateRoleDto updateDto)
        {
            var updatedRoleResult = await _authService.UpdateRoleAsync(User, updateDto);
            if (updatedRoleResult.IsSucceed)
            {
                return Ok(updatedRoleResult.Message);
            }
            else
            {
                return StatusCode(updatedRoleResult.StatusCode, updatedRoleResult.Message);
            }
        }

        //Route => Getting user's data from it's JWT
        [HttpPost]
        [Route("me")]
        public async Task<ActionResult<LoginServiceResponseDto>> Me([FromBody] MeDto token)
        {
            try
            {
                var me = await _authService.MeAsync(token);
                if (me is not null)
                {
                    return Ok(me);
                }
                else
                {
                    return Unauthorized("Invalid Token!");
                }
            }
            catch
            {
                return Unauthorized("Invalid Token!");
            }
        }

        //Route => List of all users with details
        [HttpPost]
        [Route("users")]
        public async Task<ActionResult<IEnumerable<UserInfoResult>>> GetUsersList()
        {
            var usersList = await _authService.GetUsersListAsync();
            return Ok(usersList);
        }

        //Route => Get a user by UserName
        [HttpGet]
        [Route("users/{userName}")]
        public async Task<ActionResult<UserInfoResult>> GetUserByUserName([FromRoute] string userName)
        {
            var user = await _authService.GetUserDetailByUserNameAsync(userName);
            if (user is not null)
            {
                return Ok(user);
            }
            else
            {
                return NotFound("Invalid UserName!");
            }
        }

        //Route => Get list of all userNames for sending message
        [HttpGet]
        [Route("usernames")]
        public async Task<ActionResult<IEnumerable<string>>> GetUserNamesList()
        {
            var list = await _authService.GetUserNamesListAsync();
            return Ok(list); 
        }
    }
}
