using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    // это контроллер собственно уже для управления users
    [Route("/")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        // принимает сервис
        private readonly IUserService _userService;

        public UsersController(IUserService userService)
        {
            _userService = userService;
        }

        // тут чето типа загрузки текущего профиля человека
        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            // берем Id из токена(из клейма)
            var userIdClaim = User.FindFirst("UserId")?.Value;
            // нету ? значит не авторизован
            if (userIdClaim is null)
                return Unauthorized();
            // пробуем запарсить
            if (!Guid.TryParse(userIdClaim, out var userId))
                return BadRequest("Invalid user id in token");
            // ищем это айди
            var user = await _userService.GetMe(userId);
            if (!user.Success)
                return NotFound();
            // возвращаем если нашли
            return Ok(user);
        }

        // разрешенно только если пользователь админ
        [Authorize(Roles = "Admin")]
        [HttpGet("users")]
        public async Task<ActionResult<List<UserDTO>>> GetUsers()
        {
            var users = await _userService.GetUsersAsync();
            return Ok(users);
        }

        // разрешенно только если пользователь админ
        [Authorize(Roles = "Admin")]
        [HttpGet("profile/{id}")]
        public async Task<ActionResult<UserDTO>> GetUserById(Guid id)
        {
            var user = await _userService.GetUserByIdAsync(id);
            return Ok(user);
        }



        // разрешенно только если пользователь админ
        [Authorize(Roles = "Admin")]
        [HttpDelete("user/delete/{id}")]
        public async Task<IActionResult> DeleteUser(Guid id)
        {
            var result = await _userService.DeleteUserAsync(id);
            if (!result.Success)
            {
                return NotFound();
            }
            return NoContent();

        }
    }
}
