

using Microsoft.AspNetCore.Identity;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    // password hashing
    public class PasswordService : IPasswordService
    {
        private readonly PasswordHasher<string> _hasher = new();


        public string HashPassword(string password)
        {
            return _hasher.HashPassword("user", password);
        }

        public bool VerifyPassword(string hashedPassword, string providedPassword)
        {
            var result = _hasher.VerifyHashedPassword("user", hashedPassword, providedPassword);
            return result == PasswordVerificationResult.Success;
        }
    }
}
