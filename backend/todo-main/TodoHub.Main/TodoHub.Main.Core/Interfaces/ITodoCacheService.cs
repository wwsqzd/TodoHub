using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoCacheService
    {
        Task<List<TodoDTO>> GetAllTodosAsync(Guid UserId);
        Task SetTodosAsync(List<TodoDTO> todos, Guid UserId);
        Task DeleteCache(Guid userId);
    }
}