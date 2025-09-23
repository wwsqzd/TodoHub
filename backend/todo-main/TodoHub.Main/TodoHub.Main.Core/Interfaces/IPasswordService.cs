

namespace TodoHub.Main.Core.Interfaces
{
    public interface IPasswordService
    {
        string HashPassword(string password);
        bool VerifyPassword(string hashedPassword, string providedPassword);
        public Task<string> AddRefreshToken(Guid UserId);
        public string GenerateRefreshToken();
        public Task<string> RefreshToken(string token);
        public Task RevokeRefreshToken(string token);
        public string HashToken(string token);
        public Guid GetUserId(string token);

    }
}
