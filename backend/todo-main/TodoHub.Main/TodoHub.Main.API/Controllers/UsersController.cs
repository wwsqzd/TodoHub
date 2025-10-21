using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
        public async Task<IActionResult> GetProfile()
        {
            // take the ID from the token
            var userIdClaim = User.FindFirst("UserId")?.Value;
            
            if (userIdClaim is null)
                return Unauthorized();
            // parse
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user id in token");
            // search for id
            var user = await _userService.GetMe(userId);
            if (!user.Success)
                return NotFound();
            // return
            return Ok(user);
        }

        // get users
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim is null)
                return Unauthorized();
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        //get profile
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null)
                return Unauthorized();

            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }


        // delete user 
        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpDelete("user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null)
                return Unauthorized();

            var result = await _userService.DeleteUserAsync(id);

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
        public async Task<IActionResult> UserRole()
        {
            // take the ID from the token
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim is null)
                return Unauthorized();
            // parse
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user id in token");

            var res = await _userService.IsUserAdmin(userId);
            if (!res.Success) {
                return BadRequest();
            }
            return Ok(res);
        }
    }
}
