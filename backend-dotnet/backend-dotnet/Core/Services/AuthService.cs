using backend_dotnet.Core.Constants;
using backend_dotnet.Core.Dtos.Auth;
using backend_dotnet.Core.Dtos.General;
using backend_dotnet.Core.Entities;
using backend_dotnet.Core.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace backend_dotnet.Core.Services
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ILogService _logService;
        private readonly IConfiguration _configuration;
        public AuthService(UserManager<ApplicationUser> userManager, RoleManager<IdentityRole> roleManager, ILogService logService, IConfiguration configuration)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _logService = logService;
            _configuration = configuration;
        }

        public async Task<GeneralServiceResponseDto> SeedRoleAsync()
        {
            bool isOwnerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.OWNER);
            bool isAdminRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.ADMIN);
            bool isManagerRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.MANAGER);
            bool isUserRoleExists = await _roleManager.RoleExistsAsync(StaticUserRoles.USER);

            if(isOwnerRoleExists && isAdminRoleExists && isManagerRoleExists && isUserRoleExists)
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = true,
                    StatusCode = 200,
                    Message = "Roles Seeding is already done!"
                };
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.OWNER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.ADMIN));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.MANAGER));
            await _roleManager.CreateAsync(new IdentityRole(StaticUserRoles.USER));

            return new GeneralServiceResponseDto()
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "Roles Seeding done successfully!"
            };
        }

        public async Task<GeneralServiceResponseDto> RegisterAsync(RegisterDto registerDto)
        {
           var isUserExists = await _userManager.FindByNameAsync(registerDto.UserName);
            if (isUserExists is not null)
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 409,
                    Message = "UserName Already Exists!"
                };
            ApplicationUser newUser = new ApplicationUser()
            {
                UserName = registerDto.UserName,
                Email = registerDto.Email,
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Address = registerDto.Address,
                SecurityStamp = Guid.NewGuid().ToString(),
            };
            var createUserResult = await _userManager.CreateAsync(newUser, registerDto.Password);
            if (!createUserResult.Succeeded)
            {
                var errorString = "User Creation Failed!";
                foreach (var error in createUserResult.Errors)
                {
                    errorString += " # " + error.Description;
                }
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 400,
                    Message = errorString
                };
            }
            //Add a Default USER ROLE to all users
            await _userManager.AddToRoleAsync(newUser, StaticUserRoles.USER);
            await _logService.SaveNewLog(newUser.UserName, "Registered to Website!");

            return new GeneralServiceResponseDto()
            {
                IsSucceed = true,
                StatusCode = 201,
                Message = "User Created Successfully!"
            };

        }

        public async  Task<LoginServiceResponseDto?> LoginAsync(LoginDto loginDto)
        {
            //Find user with usernmae
            var user = await _userManager.FindByNameAsync(loginDto.UserName);
            if(user is null)
                return null;

            //Check password for user
            var isPasswordCorrect = await _userManager.CheckPasswordAsync(user, loginDto.Password);
            if(!isPasswordCorrect)
                return null;

            //Return Token and User Info to Front-end
            var newToken = await GenerateJWTTokenAsync(user);
            var roles =  await _userManager.GetRolesAsync(user);
            var userInfo =  GenerateUserInfoObject(user, roles);

            await _logService.SaveNewLog(user.UserName, "New Login");
            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo
            };

        }

        public async Task<GeneralServiceResponseDto> UpdateRoleAsync(ClaimsPrincipal User, UpdateRoleDto updateRoleDto)
        {
            var user = await _userManager.FindByNameAsync(updateRoleDto.UserName);
            if (user is null)
                return new GeneralServiceResponseDto()
                {
                    IsSucceed = false,
                    StatusCode = 404,
                    Message = "Invalid Username!"
                };

            var userRoles = await _userManager.GetRolesAsync(user);

            // Just the ADMIN and OWNER can update roles
            if (User.IsInRole(StaticUserRoles.ADMIN))
            {
                // User is admin
                if (updateRoleDto.NewRole == RoleType.USER || updateRoleDto.NewRole == RoleType.MANAGER)
                {
                    // Admins can change the role for everyone except for admins and owners
                    if (userRoles.Any(q => q.Equals(StaticUserRoles.OWNER) || q.Equals(StaticUserRoles.ADMIN)))
                    {
                        return new GeneralServiceResponseDto()
                        {
                            IsSucceed = false,
                            StatusCode = 403,
                            Message = "You aren't allowed to change the role of this user!"
                        };
                    }
                    else
                    {
                        await _userManager.RemoveFromRolesAsync(user, userRoles);
                        await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                        await _logService.SaveNewLog(user.UserName, "User Roles Updated!");
                        return new GeneralServiceResponseDto()
                        {
                            IsSucceed = true,
                            StatusCode = 200,
                            Message = "Role Updated Successfully!"
                        };
                    }
                }
                else
                {
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 403,
                        Message = "You aren't allowed to change the role of this user!"
                    };
                }
            }
            else
            {
                // User is OWNER
                if (userRoles.Any(q => q.Equals(StaticUserRoles.OWNER)))
                {
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = false,
                        StatusCode = 403,
                        Message = "You aren't allowed to change the role of this user!"
                    };
                }
                else
                {
                    await _userManager.RemoveFromRolesAsync(user, userRoles);
                    await _userManager.AddToRoleAsync(user, updateRoleDto.NewRole.ToString());
                    await _logService.SaveNewLog(user.UserName, "User Roles Updated!");
                    return new GeneralServiceResponseDto()
                    {
                        IsSucceed = true,
                        StatusCode = 200,
                        Message = "Role Updated Successfully!"
                    };
                }
            }
        }

        public async Task<LoginServiceResponseDto?> MeAsync(MeDto meDto)
        {
            ClaimsPrincipal handler = new JwtSecurityTokenHandler().ValidateToken(meDto.Token, new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = _configuration["JWT:ValidIssuer"],
                ValidAudience = _configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]))
            }, out SecurityToken securityToken);

            string decodedUserName = handler.Claims.First(q => q.Type == ClaimTypes.Name).Value;
            if(decodedUserName is null)
                return null;

            var user = await _userManager.FindByNameAsync(decodedUserName);
            if (user is null)
                return null;

            var newToken = await GenerateJWTTokenAsync(user);
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);
            await _logService.SaveNewLog(user.UserName, "NewToken Generated!");


            return new LoginServiceResponseDto()
            {
                NewToken = newToken,
                UserInfo = userInfo
            };
        }

        public async Task<IEnumerable<UserInfoResult>> GetUsersListAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            List<UserInfoResult> userInfoResult = new List<UserInfoResult>();

            foreach(var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var userInfo = GenerateUserInfoObject(user, roles);
                userInfoResult.Add(userInfo);
            }
            return userInfoResult;
        }

        public async Task<UserInfoResult?> GetUserDetailByUserNameAsync(string userName)
        {
            var user = await _userManager.FindByNameAsync(userName);
            if (user is null) return null;
            var roles = await _userManager.GetRolesAsync(user);
            var userInfo = GenerateUserInfoObject(user, roles);

            return userInfo;
        }

        public async Task<IEnumerable<string>> GetUserNamesListAsync()
        {
            var userNames = await _userManager.Users
                .Select(x => x.UserName)
                .ToListAsync();
            return userNames;
        }

        private async Task<string> GenerateJWTTokenAsync(ApplicationUser user)
        {
            var userRoles = await _userManager.GetRolesAsync(user);

            var authClaims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.NameIdentifier, user.Id),
                new Claim("FirstName", user.FirstName),
                new Claim("LastName", user.LastName),
            };
            foreach (var userRole in userRoles)
            {
                authClaims.Add(new Claim(ClaimTypes.Role, userRole));
            }

            var authSecret = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Secret"]));
            var signingCredentials = new SigningCredentials(authSecret, SecurityAlgorithms.HmacSha256);

            var tokenObject = new JwtSecurityToken(
                issuer: _configuration["JWT:ValidIssuer"],
                audience: _configuration["JWT:ValidIssuer"],
                notBefore: DateTime.Now,
                expires: DateTime.Now.AddHours(3),
                claims: authClaims,
                signingCredentials: signingCredentials
                );
            string token = new JwtSecurityTokenHandler().WriteToken(tokenObject);
            return token;
        }

        private UserInfoResult GenerateUserInfoObject(ApplicationUser user, IEnumerable<string> Roles)
        {
            return new UserInfoResult()
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                UserName = user.UserName,
                Email = user.Email,
                CreatedAt = user.CreatedAt,
                Roles = Roles
            };
        }
    }
}
