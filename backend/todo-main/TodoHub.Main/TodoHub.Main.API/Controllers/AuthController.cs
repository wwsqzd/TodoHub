
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
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
            var (token, response)= await _userService.LoginUserAsync(dto);

            // Refresh Token
            //Response.Cookies.Delete("refreshToken");
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // only in dev
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });

            if (!response.Success)
            {
                return Unauthorized(new { message = response.Error });
            }
            // return
            return Ok(response);
        }

        //"auth/register"
        [HttpPost("register")]
        [EnableRateLimiting("SignUpPolicy")]
        public async Task<IActionResult> AddUser([FromBody] RegisterDTO user)
        {
            var result = await _userService.AddUserAsync(user);
            if (!result.Success)
            {
                return Conflict(result);
            }
            return Ok(result);
        }

        // "auth/refresh"
        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshPolicy")]
        public async Task<IActionResult> RefreshToken()
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                Log.Error("Refresh token not found");
                return BadRequest(new { error = "Refresh token not found", code = 123 });
            }
            // logger
            Log.Information($"Data in Refresh Login. Refresh token: {refreshToken}");
            var (token, response) = await _userService.RefreshLoginAsync(refreshToken);

            if (!response.Success)
            {
                Log.Error(response.Error);
                return Conflict(response);
            }

            // new Refresh Token
            //Response.Cookies.Delete("refreshToken");
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,  // only in dev
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Ok(response);
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
            Response.Cookies.Append("refreshToken", "", new CookieOptions
            {
                HttpOnly = true,
                Secure = true, // false for dev
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1),
                SameSite = SameSiteMode.None
            });
            return Ok(result.Success);
        }
    }
}
