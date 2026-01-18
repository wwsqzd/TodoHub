namespace TodoHub.Main.Core.Interfaces
{
    public interface IBulkhead
    {
        string Name { get; }

        Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct);
    }
}