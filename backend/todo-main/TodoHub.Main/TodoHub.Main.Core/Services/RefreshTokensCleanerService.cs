
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

        public async Task CleanAllRefreshTokens()
        {
            await _refreshTokenRepository.DeleteOldTokensRepo();
        }
    }
}
