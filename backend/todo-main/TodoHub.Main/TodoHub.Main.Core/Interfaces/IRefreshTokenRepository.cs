
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenRepo(string refreshToken, Guid userId);
        Task RefreshTokenRepo(string refreshToken, string newToken, Guid userId);
        Task RevokeRefreshTokenRepo(string refreshToken);
        Task DeleteOldTokensRepo();
        Task<RefreshTokenEntity?> GetToken(string refreshToken);
        Task<Guid?> GetUserIdRepo(string hash_token);
    }
}