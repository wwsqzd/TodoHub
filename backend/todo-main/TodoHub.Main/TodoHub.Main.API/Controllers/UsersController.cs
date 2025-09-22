using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    // users controller
    [Route("/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
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



        // only allowed if the user is an administrator
        [Authorize(Roles = "Admin")]
        [HttpDelete("user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;

            if (userIdClaim is null)
                return Unauthorized();
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();

        }
    }
}
