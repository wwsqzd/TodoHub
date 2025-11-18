
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Serilog;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    // Authentication and authorization controller
    [Route("/api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IGoogleAuthService _googleAuthService;

        public AuthController(IUserService userService, IGoogleAuthService googleService)
        {
            _userService = userService;
            _googleAuthService = googleService;
        }

        // "auth/login"
        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto)
        {
            var (token, response)= await _userService.LoginUserAsync(dto);
            
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            
            if (token != null)
            {
                //Logger
                Log.Information($"The user {dto.Email} has logged in.");

                // Refresh Token
                Response.Cookies.Append("refreshToken", token, new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.None,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(7)
                });
            }

            // return
            return Ok(response);
        }

        // Google Auth
        [HttpGet("login/google")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> LoginGoogle()
        {
            var redirectUrl = _googleAuthService.GetGoogleLoginUrl();
            return Redirect(redirectUrl);
        }

        // Google Auth
        [HttpGet("login/google/callback")]
        public async Task<IActionResult> LoginGoogleCallBack([FromQuery] string code)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("No code received from Google");
            }

            var (refreshToken, accessToken) = await _googleAuthService.HandleGoogleCallbackAsync(code);
            if (refreshToken == "" && accessToken == null) return Unauthorized();

            Response.Cookies.Append("accessToken", accessToken.Value.Token, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddHours(2),
            });
            Response.Cookies.Append("refreshToken", refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                Path = "/",
                SameSite = SameSiteMode.None,
                Expires = DateTime.UtcNow.AddDays(7)
            });

            return Redirect("http://localhost:3000/profile");
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
            // Logger 
            if (result.Success)
            {
                Log.Information($"The user {result.Value.Email} has registered in.");
            }
            return Created();
        }

        // "auth/refresh"
        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshPolicy")]
        public async Task<IActionResult> RefreshToken()
        {
            Log.Information($"[REFRESH REQUEST] Received refresh token request at {DateTime.UtcNow}");
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                //Logger
                Log.Error("Refresh token not found in Refresh ");
                return Unauthorized("Refresh token not found");
            }
            
            var (token, response) = await _userService.RefreshLoginAsync(refreshToken);

            if (!response.Success)
            {
                // Logger
                Log.Error($"Error in refresh token service. Error message: {response.Error}");
                Response.Cookies.Append("refreshToken", "", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    Path = "/",
                    Expires = DateTime.UtcNow.AddDays(-1),
                    SameSite = SameSiteMode.None
                });
                return Unauthorized(response);
            }

            // new Refresh Token
            Response.Cookies.Append("refreshToken", token, new CookieOptions
            {
                HttpOnly = true,
                Secure = true,  
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
                //Logger
                Log.Error("Refresh token not found in Log Out ");
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
                Secure = true, 
                Path = "/",
                Expires = DateTime.UtcNow.AddDays(-1),
                SameSite = SameSiteMode.None
            });
            return Ok(result.Success);
        }
    }
}
