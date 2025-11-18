using Microsoft.AspNetCore.Identity;
using Serilog;
using System.Security.Cryptography;
using System.Text;
using TodoHub.Main.Core.Interfaces;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.Core.Services
{
    // password hashing
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<string> _hasher = new();
        private readonly IRefreshTokenRepository _refreshTokenRepository;

        public PasswordService(IRefreshTokenRepository refreshTokenRepository) 
        {
            _refreshTokenRepository = refreshTokenRepository;
        }


        public string HashPassword(string password)
        {
            return _hasher.HashPassword("user", password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword("user", hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }


        public async Task<string> AddRefreshToken(Guid UserId)
        {
            // Ich gebe dem Benutzer den normalen Token zurück und speichere den Hash in der Datenbank.
            var token = GenerateRefreshToken();
            var hash_token = HashToken(token);
            await _refreshTokenRepository.AddRefreshTokenRepo(hash_token, UserId);
            return token;
        }

        public async Task<string?> RefreshToken(string old_refresh_token, Guid userId)
        {
            var oldHash = HashToken(old_refresh_token);

            var oldToken = await _refreshTokenRepository.GetTokenRepo(oldHash);
            if (oldToken == null)
            {
                Log.Error("Old token is Invalid (in RefreshToken // PasswordService)");
                return null;
            }

            var newToken = GenerateRefreshToken();
            var newHash = HashToken(newToken);

            await _refreshTokenRepository.RefreshTokenRepo(oldHash, newHash, userId);

            return newToken;
        }



        public string GenerateRefreshToken()
        {
            var bytes = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return Convert.ToBase64String(bytes);
        }


        public string HashToken(string token)
        {
            using var sha = SHA256.Create();
            var hash = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToBase64String(hash);
        }

        public async Task<Guid?> GetUserId(string token)
        {
            string hash_token = HashToken(token);
           return await _refreshTokenRepository.GetUserIdRepo(hash_token);
        }

        public async Task RevokeRefreshToken(string token)
        {
            var hash_token = HashToken(token);
            var temp = await _refreshTokenRepository.GetTokenRepo(hash_token);
            if (temp != null && temp.IsActive)
            {
                await _refreshTokenRepository.RevokeRefreshTokenRepo(hash_token);
            }
        }

        public async Task<bool> isRefreshTokenValid(string token)
        {
            var hash_token = HashToken(token);
            return await _refreshTokenRepository.isRefreshTokenValidRepo(hash_token);
        }
    }
}
