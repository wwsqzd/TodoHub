
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class RefreshTokensCleanerService : IRefreshTokensCleanerService
    {
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly DbBulkhead _dbBulkhead;
        public RefreshTokensCleanerService(IRefreshTokenRepository rep, DbBulkhead dbBulkhead)
        {
            _refreshTokenRepository = rep;
            _dbBulkhead = dbBulkhead;
        }

        public async Task CleanAllRefreshTokens(CancellationToken ct)
        {
            await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => _refreshTokenRepository.DeleteOldTokensRepo(t), TimeSpan.FromSeconds(5), bct), ct);
        }
    }
}
