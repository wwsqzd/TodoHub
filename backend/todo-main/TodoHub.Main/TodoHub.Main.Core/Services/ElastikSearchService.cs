
using Serilog;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class ElastikSearchService : IElastikSearchService
    {
        private readonly IElastikSearchRepository _repository;

        public ElastikSearchService(IElastikSearchRepository repository)
        {
            _repository = repository;
        }

        public async Task<Result<bool>> UpsertDoc(TodoDTO todo, Guid todoId)
        {
            try
            {
                Log.Information("UpsertDoc starting in ESS");
                var res = await _repository.UpsertDocRepo(todo, todoId);
                return res
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to upsert document");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        public async Task<Result<bool>> CreateIndex()
        {
            try
            {
                Log.Information("CreateIndex starting in ESS");
                var response = await _repository.CreateIndexRepo();

                return response
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to create index");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }


        public async Task<Result<bool>> DeleteDoc(Guid todoId, Guid ownerId)
        {
            try
            {
                Log.Information("DeleteDoc starting in ESS");
                var res = await _repository.DeleteDocRepo(todoId, ownerId);
                return res
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to delete document");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        public async Task<Result<bool>> ReIndex()
        {
            try
            {
                Log.Information("ReIndex starting in ESS");
                var res = await _repository.ReIndexRepo();
                return res
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to reindex");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        public async Task<Result<List<TodoDTO>>> SearchDocuments(Guid userId, string query)
        {
            try
            {
                Log.Information("SearchDocuments starting in ESS");
                var res = await _repository.SearchDocumentsRepo(userId, query);
                return Result<List<TodoDTO>>.Ok(res ?? new List<TodoDTO>());
            }
            catch (Exception ex)
            {
                return Result<List<TodoDTO>>.Fail($"Exception: {ex.Message}");
            }
        }
    }
}
