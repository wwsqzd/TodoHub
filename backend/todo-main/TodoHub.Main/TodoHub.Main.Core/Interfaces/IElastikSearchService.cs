using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IElastikSearchService
    {
        Task<Result<bool>> CreateIndex();
        Task<Result<bool>> ReIndex();
        Task<Result<bool>> UpsertDoc(TodoDTO todo, Guid todoId, CancellationToken ct);
        Task<Result<List<TodoDTO>>> SearchDocuments(Guid userId, string query, CancellationToken ct);
        Task<Result<bool>> DeleteDoc(Guid TodoId, Guid OwnerId, CancellationToken ct);
        
    }
}
