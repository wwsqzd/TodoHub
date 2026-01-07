
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class RefreshTokensCleanerService : IRefreshTokensCleanerService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        public RefreshTokensCleanerService(IRefreshTokenRepository rep)
        {
            _refreshTokenRepository = rep;
        }

        public async Task CleanAllRefreshTokens(CancellationToken ct)
        {
            await ResilienceExecutor.WithTimeout(t => _refreshTokenRepository.DeleteOldTokensRepo(t), TimeSpan.FromSeconds(5), ct);
        }
    }
}
