
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenRepo(string refreshToken, Guid userId, CancellationToken ct);
        Task RefreshTokenRepo(string refreshToken, string newToken, Guid userId, CancellationToken ct);
        Task RevokeRefreshTokenRepo(string refreshToken, CancellationToken ct);
        Task DeleteOldTokensRepo(CancellationToken ct);
        Task<RefreshTokenEntity?> GetTokenRepo(string refreshToken, CancellationToken ct);
        Task<Guid?> GetUserIdRepo(string hash_token, CancellationToken ct);
        Task<bool> DeleteRefreshTokensByUserRepo(Guid userId, CancellationToken ct);
        Task<bool> isRefreshTokenValidRepo(string refreshToken, CancellationToken ct);
    }
}