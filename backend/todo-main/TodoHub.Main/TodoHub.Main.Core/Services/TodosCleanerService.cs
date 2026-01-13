using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class TodosCleanerService : ITodosCleanerService
    {
        private readonly ITodoRepository _repository;
        private readonly DbBulkhead _dbBulkhead;

        public TodosCleanerService(ITodoRepository repository, DbBulkhead dbBulkhead)
        {
            _repository = repository;
            _dbBulkhead = dbBulkhead;
        }
        public async Task CleanALlTodosByUser(Guid ownerId, CancellationToken ct)
        {
            await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _repository.DeleteAllTodoByUserAsyncRepo(ownerId, t), TimeSpan.FromSeconds(5), bct), ct);
        }
    }
}
