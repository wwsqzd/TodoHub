
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Request;
using TodoHub.Main.Core.DTOs.Response;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IElastikSearchService
    {
        Task<Result<bool>> CreateIndex();
        Task<Result<bool>> ReIndex();
        Task<Result<bool>> UpsertDoc(TodoDTO todo, Guid todoId);
        Task<Result<List<TodoDTO>>> SearchDocuments(Guid userId, string query);
        Task<Result<bool>> DeleteDoc(Guid TodoId, Guid OwnerId);
        
    }
}
