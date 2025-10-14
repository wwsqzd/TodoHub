

using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoRepository
    {
        Task<TodoDTO> AddTodoAsyncRepo(CreateTodoDTO todo, Guid OwnerId);
        Task<Guid?> DeleteTodoAsyncRepo(Guid id, Guid OwnerId);
        Task<bool> DeleteAllTodoByUserAsyncRepo(Guid OwnerId);
        Task<TodoDTO?> GetTodoByIdAsyncRepo(Guid id, Guid OwnerId);
        Task<TodoDTO> UpdateTodoAsyncRepo(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId);
        Task<List<TodoDTO>> GetTodosAsyncRepo(Guid UserId);
    }
}
