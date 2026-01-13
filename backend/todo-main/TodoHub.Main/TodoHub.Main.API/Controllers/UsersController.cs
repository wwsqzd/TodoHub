using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    // users controller
    [Route("/api/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        
        private readonly IUserService _userService;
        private readonly IQueueProducer _queueProducer;

        public UsersController(IUserService userService, IQueueProducer producer)
        {
            _userService = userService;
            _queueProducer = producer;
        }


        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile(CancellationToken ct)
        {
            // take the ID from the token
            var userIdClaim = User.FindFirst("UserId")?.Value;
            
            if (userIdClaim is null)
                return Unauthorized();
            // parse
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user id in token");
            // search for id
            var user = await _userService.GetMe(userId, ct);
            if (!user.Success)
                return NotFound();
            // return
            return Ok(user);
        }

        // get users
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDTO>>> GetUsers(CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim is null)
                return Unauthorized();
            var users = await _userService.GetUsersAsync(ct);
            if (!users.Success)
                return NotFound();
            return Ok(users);
        }

        //get profile
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null)
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(id, ct);
            if (!user.Success)
                return NotFound();
            return Ok(user);
        }


        // delete user 
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpDelete("user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null)
                return Unauthorized();

            var result = await _userService.DeleteUserAsync(id, ct);

            var dto = new MessageEnvelope{ 
                Command = "Clean Todos By User", 
                UserId = id 
            };
            _queueProducer.Send(dto);

            if (!result.Success)
            {
                return Conflict();
            }
            return Ok(result);

        }

        [HttpGet("profile/role")]
        [Authorize]
        public async Task<IActionResult> UserRole(CancellationToken ct)
        {
            // take the ID from the token
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim is null)
                return Unauthorized();
            // parse
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user id in token");

            var res = await _userService.IsUserAdmin(userId, ct);
            if (!res.Success) {
                return BadRequest();
            }
            return Ok(res);
        }


        [HttpPatch("profile/language")]
        [Authorize]
        public async Task<IActionResult> ChangeUserLanguage([FromBody] ChangeLanguageDTO language_dto, CancellationToken ct)
        {
            // take the ID from the token
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var res = await _userService.ChangeUserLanguage(language_dto, userId, ct);
            if (!res.Success)
            {
                return BadRequest();
            }
            return Ok(res);
        }
    }
}
