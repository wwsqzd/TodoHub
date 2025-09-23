

using Microsoft.AspNetCore.Identity;
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
            var token = GenerateRefreshToken();
            var hash_token = HashToken(token);
            await _refreshTokenRepository.AddRefreshTokenRepo(hash_token, UserId);
            return token;
        }

        public async Task<string> RefreshToken(string token)
        {
            var old_hash_token = HashToken(token);
            var old_token = _refreshTokenRepository.GetToken(old_hash_token);
            if (old_token != null && old_token.IsActive)
            {
                var new_token = GenerateRefreshToken();
                var new_hash_token = HashToken(new_token);
                
                await _refreshTokenRepository.RefreshTokenRepo(old_hash_token, new_hash_token);
                return new_token;
            }
            throw new Exception("Not Valid");
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

        public Guid GetUserId(string token)
        {
            var hash_token = HashToken(token);
            return _refreshTokenRepository.GetUserIdRepo(hash_token);
        }

        public async Task RevokeRefreshToken(string token)
        {
            var hash_token = HashToken(token);
            var temp = _refreshTokenRepository.GetToken(hash_token);
            if (temp != null && temp.IsActive)
            {
                await _refreshTokenRepository.RevokeRefreshTokenRepo(hash_token);
            }
        }
    }
}
