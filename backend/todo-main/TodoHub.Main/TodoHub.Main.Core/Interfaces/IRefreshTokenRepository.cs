
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<bool> AddRefreshTokenRepo(string refreshToken, Guid userId, CancellationToken ct);
        Task<bool> RefreshTokenRepo(string refreshToken, string newToken, Guid userId, CancellationToken ct);
        Task<bool> RevokeRefreshTokenRepo(string refreshToken, CancellationToken ct);
        Task<bool> DeleteOldTokensRepo(CancellationToken ct);
        Task<RefreshTokenEntity?> GetTokenRepo(string refreshToken, CancellationToken ct);
        Task<Guid?> GetUserIdRepo(string hash_token, CancellationToken ct);
        Task<bool> DeleteRefreshTokensByUserRepo(Guid userId, CancellationToken ct);
        Task<bool> isRefreshTokenValidRepo(string refreshToken, CancellationToken ct);
    }
}