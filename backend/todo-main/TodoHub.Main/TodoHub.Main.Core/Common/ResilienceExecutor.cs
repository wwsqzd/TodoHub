
namespace TodoHub.Main.Core.Common
{
    public static class ResilienceExecutor
    {
        public static async Task<T> WithTimeout<T>(
            Func<CancellationToken, Task<T>> action,
            TimeSpan timeout,
            CancellationToken ct)
        {
            using var timeoutcts = new CancellationTokenSource(timeout);
            using var linkedcts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutcts.Token);

            try
            {
                return await action(linkedcts.Token);
            }
            catch (OperationCanceledException) when (timeoutcts.IsCancellationRequested)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds}s");
            }
        }

        public static async Task WithTimeout(
            Func<CancellationToken, Task> action,
            TimeSpan timeout,
            CancellationToken ct)
        {
            using var timeoutcts = new CancellationTokenSource(timeout);
            using var linkedcts = CancellationTokenSource.CreateLinkedTokenSource(ct, timeoutcts.Token);

            try
            {
                await action(linkedcts.Token);
            }
            catch (OperationCanceledException) when (timeoutcts.IsCancellationRequested)
            {
                throw new TimeoutException($"Operation timed out after {timeout.TotalSeconds}s");
            }
        }
    }

}
