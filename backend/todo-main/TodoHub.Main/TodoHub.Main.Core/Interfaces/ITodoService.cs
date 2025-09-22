

using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoService
    {
        Task<Result<CreateTodoDTO>> AddTodoAsync(CreateTodoDTO todo, Guid OwnerId);
        Task<Result<bool>> DeleteTodoAsync(Guid id, Guid OwnerId);

        Task<Result<TodoDTO>> GetTodoByIdAsync(Guid id, Guid OwnerId);
        Task<Result<UpdateTodoDTO>> UpdateTodoAsync(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId);
        Task<Result<List<TodoDTO>>> GetTodosAsync(Guid UserId);
    }
}
