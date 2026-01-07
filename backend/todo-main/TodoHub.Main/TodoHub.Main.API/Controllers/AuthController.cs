
using Microsoft.AspNetCore.Http.Timeouts;
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
        private readonly IGitHubAuthService _gitHubAuthService;

        public AuthController(IUserService userService, IGoogleAuthService googleService, IGitHubAuthService gitHubAuthService)
        {
            _userService = userService;
            _googleAuthService = googleService;
            _gitHubAuthService = gitHubAuthService;
        }

        // "auth/login"
        [HttpPost("login")]
        [EnableRateLimiting("LoginPolicy")]
        public async Task<IActionResult> Login([FromBody] LoginDTO dto, CancellationToken ct)
        {
            var (token, response)= await _userService.LoginUserAsync(dto, ct);
            
            if (!response.Success)
            {
                return Unauthorized(response);
            }

            
            if (token != null)
            {
                //Logger
                //Log.Information($"The user {dto.Email} has logged in.");

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
        public IActionResult LoginGoogle(CancellationToken ct)
        {
            var redirectUrl = _googleAuthService.GetGoogleLoginUrl();
            return Redirect(redirectUrl);
        }

        // Google Auth
        [HttpGet("login/google/callback")]
        public async Task<IActionResult> LoginGoogleCallBack([FromQuery] string code, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("No code received from Google");
            }

            var (refreshToken, accessToken) = await _googleAuthService.HandleGoogleCallbackAsync(code, ct);
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


        //GitHub Auth
        [HttpGet("login/github")]
        public IActionResult LoginGitHub(CancellationToken ct)
        {
            var redirectUrl = _gitHubAuthService.GetGitHubLoginUrl();
            return Redirect(redirectUrl);
        }

        // GitHub Auth
        [HttpGet("login/github/callback")]
        public async Task<IActionResult> LoginGitHubCallBack([FromQuery] string code, CancellationToken ct)
        {
            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("No code received from Github");
            }

            var (refreshToken, accessToken) = await _gitHubAuthService.HandleGitHubCallbackAsync(code, ct);
            if (accessToken.Success == false) return BadRequest($"{accessToken.Error}");
            if (refreshToken == "" && accessToken.Value.Token == null) return Unauthorized();

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
        public async Task<IActionResult> AddUser([FromBody] RegisterDTO user, CancellationToken ct)
        {
            var result = await _userService.AddUserAsync(user, ct);
            if (!result.Success)
            {
                return Conflict(result);
            }
            // Logger 
            if (result.Success)
            {
                //Log.Information($"The user {result.Value.Email} has registered in.");
            }
            return Created();
        }

        // "auth/refresh"
        [HttpPost("refresh")]
        [EnableRateLimiting("RefreshPolicy")]
        public async Task<IActionResult> RefreshToken(CancellationToken ct)
        {
            Log.Information($"[REFRESH REQUEST] Received refresh token request at {DateTime.UtcNow}");
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                //Logger
                Log.Error("Refresh token not found in Refresh ");
                return Unauthorized("Refresh token not found");
            }
            
            var (token, response) = await _userService.RefreshLoginAsync(refreshToken, ct);

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
        public async Task<IActionResult> Logout(CancellationToken ct)
        {
            if (!Request.Cookies.TryGetValue("refreshToken", out var refreshToken))
            {
                //Logger
                Log.Error("Refresh token not found in Log Out ");
                return Unauthorized("Refresh token not found");
            }
            var result = await _userService.LogoutUserAsync(refreshToken, ct);
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
