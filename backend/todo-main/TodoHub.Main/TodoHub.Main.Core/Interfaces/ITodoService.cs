

using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoService
    {
        Task<Result<TodoDTO>> AddTodoAsync(CreateTodoDTO todo, Guid OwnerId);
        Task<Result<Guid>> DeleteTodoAsync(Guid id, Guid OwnerId);

        Task<Result<TodoDTO>> GetTodoByIdAsync(Guid id, Guid OwnerId);
        Task<Result<TodoDTO>> UpdateTodoAsync(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId);
        Task<Result<List<TodoDTO>>> GetTodosAsync(Guid UserId);
    }
}
