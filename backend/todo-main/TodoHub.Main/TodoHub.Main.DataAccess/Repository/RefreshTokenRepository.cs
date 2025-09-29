
using Microsoft.EntityFrameworkCore;
using TodoHub.Main.Core.Entities;
using TodoHub.Main.DataAccess.Context;
using TodoHub.Main.DataAccess.Interfaces;

namespace TodoHub.Main.DataAccess.Repository
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly ApplicationDbContext _context;

        public RefreshTokenRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task AddRefreshTokenRepo(string refreshToken, Guid userId)
        {
            var entity = new RefreshTokenEntity
            {
                UserId = userId,
                TokenHash = refreshToken
            };
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteOldTokensRepo()
        {
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var oldTokens = await _context.RefreshTokens.Where(token => token.Expires < tomorrow || token.Revoked != null).ToListAsync();
            _context.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();
        }

        public RefreshTokenEntity GetToken(string refreshToken)
        {
            var res = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == refreshToken);
            return res!;
        }

        public Guid GetUserIdRepo(string hash_token)
        {
            var res = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == hash_token);
            return res!.UserId;
        }

        public async Task RefreshTokenRepo(string refreshToken, string newToken)
        {
            var res = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == refreshToken);
            if (res != null)
            {
                res.Revoked = DateTime.UtcNow;
                res.ReplacedByToken = newToken;
            }
            await _context.SaveChangesAsync();
        }


        public async Task RevokeRefreshTokenRepo(string refreshToken)
        {
            var res = _context.RefreshTokens.FirstOrDefault(t => t.TokenHash == refreshToken);
            if (res != null)
            {
                res.Revoked = DateTime.UtcNow;
                await _context.SaveChangesAsync();
            }
        }
    }
}
