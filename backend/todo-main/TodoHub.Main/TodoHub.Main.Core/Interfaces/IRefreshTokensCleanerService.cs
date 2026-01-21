using TodoHub.Main.Core.Common;

namespace TodoHub.Main.Core.Interfaces
{
    public interface IRefreshTokensCleanerService
    {
        Task<Result<bool>> CleanAllRefreshTokens(CancellationToken ct);
    }
}