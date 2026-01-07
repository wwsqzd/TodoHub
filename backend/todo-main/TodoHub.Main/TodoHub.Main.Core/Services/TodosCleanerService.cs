using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class TodosCleanerService : ITodosCleanerService
    {
        private readonly ITodoRepository _repository;


        public TodosCleanerService(ITodoRepository repository)
        {
            _repository = repository;
        }
        public async Task CleanALlTodosByUser(Guid ownerId, CancellationToken ct)
        {
            await ResilienceExecutor.WithTimeout(t => _repository.DeleteAllTodoByUserAsyncRepo(ownerId, t), TimeSpan.FromSeconds(5), ct);
        }
    }
}
