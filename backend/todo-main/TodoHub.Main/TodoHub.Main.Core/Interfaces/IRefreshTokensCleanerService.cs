namespace TodoHub.Main.Core.Interfaces
{
    public interface IRefreshTokensCleanerService
    {
        Task CleanAllRefreshTokens(CancellationToken ct);
    }
}