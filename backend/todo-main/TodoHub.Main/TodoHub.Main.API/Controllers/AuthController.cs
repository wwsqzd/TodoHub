
using Microsoft.AspNetCore.Mvc;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    // Authentication and authorization controller
    [Route("auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;

        public AuthController(IUserService userService)
        {
            _userService = userService;
        }

        // "auth/login"
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var token = await _userService.LoginUserAsync(dto);
            if (!token.Success)
            {
                return Unauthorized(new { message = token.Error });
            }
            // return
            return Ok(token);
        }

        //для ветки "auth/register"
        [HttpPost("register")]
        public async Task<IActionResult> AddUser([FromBody] RegisterDTO user)
        {
            var result = await _userService.AddUserAsync(user);
            if (!result.Success)
            {
                return Conflict(result.Error);
            }
            return Ok();
        }
    }
}
