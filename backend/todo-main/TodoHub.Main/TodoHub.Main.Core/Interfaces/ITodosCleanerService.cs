namespace TodoHub.Main.Core.Interfaces
{
    public interface ITodosCleanerService
    {
        Task CleanALlTodosByUser(Guid ownerId, CancellationToken ct);
    }
}