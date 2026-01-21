

using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoRepository
    {
        Task<TodoDTO> AddTodoAsyncRepo(CreateTodoDTO todo, Guid OwnerId, CancellationToken ct);
        Task<Guid?> DeleteTodoAsyncRepo(Guid id, Guid OwnerId, CancellationToken ct);
        Task<bool> DeleteAllTodoByUserAsyncRepo(Guid OwnerId, CancellationToken ct);
        Task<TodoDTO?> GetTodoByIdAsyncRepo(Guid id, Guid OwnerId, CancellationToken ct);
        Task<TodoDTO> UpdateTodoAsyncRepo(UpdateTodoDTO todo, Guid OwnerId, Guid TodoId, CancellationToken ct);
        //Task<List<TodoDTO>> GetTodosByPageAsyncRepo(Guid UserId, DateTime? lastCreated, Guid? lastId, CancellationToken ct);
        Task<List<TodoDTO>> GetTodosAsyncRepo(Guid UserId, CancellationToken ct);

    }
}
