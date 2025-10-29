using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodoCacheService
    {
        Task<List<TodoDTO>> GetTodosAsync(Guid UserId, DateTime? lastCreated = null, Guid? lastId = null);
        Task SetTodosAsync(List<TodoDTO> todos, Guid UserId);
        Task DeleteCache(Guid userId);
    }
}