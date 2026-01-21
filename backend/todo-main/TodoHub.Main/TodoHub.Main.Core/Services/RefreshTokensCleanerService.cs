
using TodoHub.Main.Core.Common;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.Core.Services
{
    public class RefreshTokensCleanerService : IRefreshTokensCleanerService
    {
        private readonly IRefreshTokenRepository tokens_repo;
        private readonly DbBulkhead _dbBulkhead;
        public RefreshTokensCleanerService(IRefreshTokenRepository rep, DbBulkhead dbBulkhead)
        {
            tokens_repo = rep;
            _dbBulkhead = dbBulkhead;
        }

        public async Task<Result<bool>> CleanAllRefreshTokens(CancellationToken ct)
        {
            try
            {
                var response = await _dbBulkhead.ExecuteAsync(bct => ResilienceExecutor.WithTimeout(t => tokens_repo.DeleteOldTokensRepo(t), TimeSpan.FromSeconds(5), bct), ct);
                return Result<bool>.Ok(response);
            }
            catch (Exception ex)
            {
                return Result<bool>.Fail(ex.Message);
            }
            
        }
    }
}
