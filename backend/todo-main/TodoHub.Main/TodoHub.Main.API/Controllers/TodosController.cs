using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    [Route("/todos")]
    [ApiController]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;
        
        public TodosController(ITodoService service)
        {
            _todoService = service;
        }


        [HttpGet]
        [EnableRateLimiting("TodosPolicy")]
        public async Task<IActionResult> GetTodos()
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var todos = await _todoService.GetTodosAsync(userId);
            if (!todos.Success)
            {
                return NotFound();
            }
            return Ok(todos);
        }


        [HttpPost("create")]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDTO dto)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.AddTodoAsync(dto, userId);
            if (!result.Success)
            {
                return BadRequest();
            }
            return Ok(result);

        }

        [HttpGet("{TodoId}")]
        public async Task<IActionResult> GetTodo(Guid TodoId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var todo = await _todoService.GetTodoByIdAsync(TodoId, userId);

            if (!todo.Success)
            {
                return BadRequest("Todo was not found");
            }
            return Ok(todo);
        }
        // Delete Todo
        [HttpDelete("{TodoId}")]
        public async Task<IActionResult> DeleteTodo(Guid TodoId)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.DeleteTodoAsync(TodoId, userId);
            
            if (!result.Success)
            {
                return BadRequest("Error by deleting");
            }
            return Ok(result);
        }

        [HttpPatch("{TodoId}")]
        public async Task<IActionResult> UpdateTodo(Guid TodoId, UpdateTodoDTO update_todo)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.UpdateTodoAsync(update_todo, userId, TodoId);

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }
            return Ok(result);
        }

    }
}
