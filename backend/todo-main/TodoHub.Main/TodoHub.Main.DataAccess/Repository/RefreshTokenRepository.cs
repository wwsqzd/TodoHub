// Refresh Token Repository

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

        // add refresh token
        public async Task AddRefreshTokenRepo(string refreshToken, Guid userId)
        {
            Log.Information("AddRefreshTokenRepo starting in RTR");
            var entity = new RefreshTokenEntity
            {
                UserId = userId,
                TokenHash = refreshToken
            };
            await _context.RefreshTokens.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        // delete old tokens
        public async Task DeleteOldTokensRepo()
        {
            Log.Information("DeleteOldTokenRepo starting in RTR");

            var tomorrow = DateTime.UtcNow.AddDays(1);
            var oldTokens = await _context.RefreshTokens.Where(token => token.Expires < tomorrow || token.Revoked != null).ToListAsync();
            _context.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();
            Log.Information("Old tokens deleted");
        }

        // get token
        public async Task<RefreshTokenEntity?> GetTokenRepo(string refreshToken)
        {
            Log.Information("GetTokenRepo starting in RTR");

            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null) return null;
            return res;
        }

        // get User id by refresh token
        public async Task<Guid?> GetUserIdRepo(string hash_token)
        {
            Log.Information("GetUserIdRepo starting in RTR");
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash_token);
            return res?.UserId;
        }

        // refresh old token
        public async Task RefreshTokenRepo(string refreshToken, string newToken, Guid userId)
        {
            Log.Information("RefreshTokenRepo starting in RTR");

            // revoke old token
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash.Equals(refreshToken));
            if (token == null)
            {
                return;
            }
            while (token!.ReplacedByToken != null)
            {
                token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == token.ReplacedByToken);
            }
            token.Revoked = DateTime.UtcNow;
            token.ReplacedByToken = newToken;
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
            Log.Information("RevokeRefreshTokenRepo starting in RTR");

            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null)
            {
                return;
            }
            res.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync();
        }

        // delete refresh tokens by user
        public async Task<bool> DeleteRefreshTokensByUserRepo(Guid userId)
        {
            Log.Information("DeleteRefreshTokensByUserRepo starting in RTR");

            var refreshTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            _context.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
            return true;
        }

        // is Refresh Token valid?
        public async Task<bool> isRefreshTokenValidRepo(string refreshToken)
        {
            Log.Information("isRefreshTokenValidRepo starting in RTR");

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);

            while (token?.ReplacedByToken != null)
            {
                token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == token.ReplacedByToken);
            }

            return token != null
                && token.ReplacedByToken == null
                && token.Expires > DateTime.UtcNow.AddSeconds(-10);
        }
    }
}
