

namespace TodoHub.Main.Core.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        Task<string> AddRefreshToken(Guid UserId, CancellationToken ct);
        string GenerateRefreshToken();
        Task<string?> RefreshToken(string token, Guid userId, CancellationToken ct);
        Task<bool> RevokeRefreshToken(string token, CancellationToken ct);
        string HashToken(string token);
        Task<Guid?> GetUserId(string token, CancellationToken ct);
        Task<bool> isRefreshTokenValid(string token, CancellationToken ct);
    }
}
