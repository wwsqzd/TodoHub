using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IElastikSearchRepository
    {
        Task<bool> CreateIndexRepo();
        Task<bool> ReIndexRepo();
        Task<bool> UpsertDocRepo(TodoDTO todo, Guid TodoId, CancellationToken ct);
        Task<List<TodoDTO>> SearchDocumentsRepo(Guid userId, string query, CancellationToken ct);
        Task<bool> DeleteDocRepo(Guid TodoId, Guid OwnerId, CancellationToken ct);
    }
}
