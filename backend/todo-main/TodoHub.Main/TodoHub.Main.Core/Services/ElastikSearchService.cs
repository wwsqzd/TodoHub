using Serilog;
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class ElastikSearchService : IElastikSearchService
    {
        private readonly IElastikSearchRepository _repository;
        private readonly EsBulkhead _esBulkhead;

        public ElastikSearchService(IElastikSearchRepository repository, EsBulkhead esBulkhead)
        {
            _repository = repository;
            _esBulkhead = esBulkhead;
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

        public async Task<Result<bool>> UpsertDoc(TodoDTO todo, Guid todoId, CancellationToken ct)
        {
            try
            {
                Log.Information("UpsertDoc starting in ESS");
                var res = await _esBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _repository.UpsertDocRepo(todo, todoId, t), TimeSpan.FromSeconds(5), bct), ct);
                return res
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to upsert document");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }


        public async Task<Result<bool>> DeleteDoc(Guid todoId, Guid ownerId, CancellationToken ct)
        {
            try
            {
                Log.Information("DeleteDoc starting in ESS");
                var res = await _esBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _repository.DeleteDocRepo(todoId, ownerId, t), TimeSpan.FromSeconds(5), bct), ct);
                return res
                    ? Result<bool>.Ok(true)
                    : Result<bool>.Fail("Failed to delete document");
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail($"Exception: {ex.Message}");
            }
        }

        

        public async Task<Result<List<TodoDTO>>> SearchDocuments(Guid userId, string query, CancellationToken ct)
        {
            try
            {
                Log.Information("SearchDocuments starting in ESS");
                var res = await _esBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _repository.SearchDocumentsRepo(userId, query, t), TimeSpan.FromSeconds(2), bct), ct);
                return Result<List<TodoDTO>>.Ok(res ?? new List<TodoDTO>());
            }
            catch (Exception ex)
            {
                return Result<List<TodoDTO>>.Fail($"Exception: {ex.Message}");
            }
        }
    }
}
