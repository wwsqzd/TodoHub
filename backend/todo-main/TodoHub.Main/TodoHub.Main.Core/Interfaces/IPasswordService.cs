

namespace TodoHub.Main.Core.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        Task<string> AddRefreshToken(Guid UserId);
        string GenerateRefreshToken();
        Task<string?> RefreshToken(string token, Guid userId);
        Task RevokeRefreshToken(string token);
        string HashToken(string token);
        Task<Guid?> GetUserId(string token);
        Task<bool> isRefreshTokenValid(string token);
    }
}
