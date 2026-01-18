using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Common
{
    public sealed class BulkheadRejektedException : Exception
    {
        public BulkheadRejektedException(string name) : base($"Bulkhead '{name}' rejekted request") { }
    }

    public class Bulkhead : IBulkhead
    {
        private readonly SemaphoreSlim _sem;
        public string Name { get; }

        public Bulkhead(string name, int MaxConcurrency)
        {
            Name = name;
            _sem = new SemaphoreSlim(MaxConcurrency);
        }

        public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> action, CancellationToken ct)
        {
            var arquired = await _sem.WaitAsync(0, ct).ConfigureAwait(false);
            if (!arquired) throw new BulkheadRejektedException(Name);

            try { return await action(ct).ConfigureAwait(false); }
            finally { _sem.Release(); }
        }
    }
}
