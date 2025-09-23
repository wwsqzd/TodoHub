
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
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
        [EnableRateLimiting("LoginPolicy")]
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

        //"auth/register"
        [HttpPost("register")]
        [EnableRateLimiting("SignUpPolicy")]
        public async Task<IActionResult> AddUser([FromBody] RegisterDTO user)
        {
            var result = await _userService.AddUserAsync(user);
            if (!result.Success)
            {
                return Conflict(result.Error);
            }
            return Ok();
        }

        // "auth/refresh"
        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshPolicy")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken ))
            {
                return Unauthorized("Refresh token not found");
            }
            var token = await _userService.RefreshLoginAsync(refreshToken);
            if (!token.Success)
            {
                return Unauthorized(token.Error);
            }
            return Ok(token);
        }

        // "auth/logout"
        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                return Unauthorized("Refresh token not found");
            }
            var result = await _userService.LogoutUserAsync(refreshToken);
            if (!result.Success)
            {
                return BadRequest(result.Error);
            }
            return Ok();
        }
    }
}
