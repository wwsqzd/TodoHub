
using TodoHub.Main.Core.Entities;

namespace TodoHub.Main.DataAccess.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task AddRefreshTokenRepo(string refreshToken, Guid userId);
        Task RefreshTokenRepo(string refreshToken, string newToken);
        Task RevokeRefreshTokenRepo(string refreshToken);
        Task DeleteOldTokensRepo();
        RefreshTokenEntity GetToken(string refreshToken);
        Guid GetUserIdRepo(string hash_token);
    }
}