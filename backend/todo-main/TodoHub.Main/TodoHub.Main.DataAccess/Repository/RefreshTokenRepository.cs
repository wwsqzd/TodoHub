
using Microsoft.EntityFrameworkCore;
using Serilog;
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
            Log.Information("Old tokens deleted");
        }

        public async Task<RefreshTokenEntity?> GetToken(string refreshToken)
        {
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null)
            {
                return null;
            }
            return res;
        }

        public async Task<Guid?> GetUserIdRepo(string hash_token)
        {
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash_token);
            return res?.UserId;
        }

        public async Task RefreshTokenRepo(string refreshToken, string newToken, Guid userId)
        {
            // revoke old token
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null)
            {
                return;
            }
            res.Revoked = DateTime.UtcNow;
            res.ReplacedByToken = newToken;
            // add new token
            var entity = new RefreshTokenEntity
            {
                UserId = userId,
                TokenHash = newToken
            };
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // revoke token
        public async Task RevokeRefreshTokenRepo(string refreshToken)
        {
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null)
            {
                return;
            }
            res.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}
