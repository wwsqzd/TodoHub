using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.Timeouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.API.Controllers
{
    [Route("/api/todos")]
    [ApiController]
    [Authorize]
    public class TodosController : ControllerBase
    {
        private readonly ITodoService _todoService;
        
        public TodosController(ITodoService service)
        {
            _todoService = service;
        }

        // Get todos
        [RequestTimeout(2000)]
        [HttpGet()]
        [EnableRateLimiting("TodosPolicy")]
        public async Task<IActionResult> GetTodos(DateTime? lastCreated, Guid? lastId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var todos = await _todoService.GetTodosAsync(userId, lastCreated, lastId, ct);
            if (!todos.Success)
            {
                return NotFound();
            }
            return Ok(todos);
        }

        // Create Todo
        [RequestTimeout(5000)]
        [HttpPost("create")]
        public async Task<IActionResult> CreateTodo([FromBody] CreateTodoDTO dto, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.AddTodoAsync(dto, userId, ct);
            if (!result.Success)
            {
                return BadRequest(result.Error);
            }
            return Ok(result);

        }

        // Get todo
        [RequestTimeout(2000)]
        [HttpGet("{TodoId}")]
        public async Task<IActionResult> GetTodo(Guid TodoId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var todo = await _todoService.GetTodoByIdAsync(TodoId, userId, ct);

            if (!todo.Success)
            {
                return BadRequest("Todo was not found");
            }
            return Ok(todo);
        }
        // Delete Todo
        [RequestTimeout(5000)]
        [HttpDelete("{TodoId}")]
        public async Task<IActionResult> DeleteTodo(Guid TodoId, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.DeleteTodoAsync(TodoId, userId, ct);
            
            if (!result.Success)
            {
                return BadRequest("Error by deleting");
            }
            return Ok(result);
        }

        // Modify Todo
        [RequestTimeout(5000)]
        [HttpPatch("{TodoId}")]
        public async Task<IActionResult> UpdateTodo(Guid TodoId, UpdateTodoDTO update_todo, CancellationToken ct)
        {
            var userIdClaim = User.FindFirst("UserId")?.Value;
            if (userIdClaim is null || !Guid.TryParse(userIdClaim, out var userId))
                return Unauthorized("Invalid token");

            var result = await _todoService.UpdateTodoAsync(update_todo, userId, TodoId, ct);

            if (!result.Success)
            {
                return BadRequest(result.Error);
            }
            return Ok(result);
        }

    }
}
