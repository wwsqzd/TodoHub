
using TodoHub.Main.Core.Common;

namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodosCleanerService
    {
        Task<Result<bool>> CleanALlTodosByUser(Guid ownerId, CancellationToken ct);
    }
}