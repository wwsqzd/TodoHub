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
            var tomorrow = DateTime.UtcNow.AddDays(1);
            var oldTokens = await _context.RefreshTokens.Where(token => token.Expires < tomorrow || token.Revoked != null).ToListAsync();
            _context.RemoveRange(oldTokens);
            await _context.SaveChangesAsync();
            Log.Information("Old tokens deleted");
        }

        // get token
        public async Task<RefreshTokenEntity?> GetTokenRepo(string refreshToken)
        {
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken);
            if (res == null)
            {
                return null;
            }
            return res;
        }

        // get User id by refresh token
        public async Task<Guid?> GetUserIdRepo(string hash_token)
        {
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash_token);
            return res?.UserId;
        }

        // refresh old token
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

        // delete refresh tokens by user
        public async Task<bool> DeleteRefreshTokensByUserRepo(Guid userId)
        {
            var refreshTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync();
            _context.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync();
            return true;
        }

        // is Refresh Token valid?
        public async Task<bool> isRefreshTokenValidRepo(string refreshToken)
        {
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash.Equals(refreshToken));
            Log.Information($"[REFRESH CHECK] Token expires at: {token?.Expires:o}, Now: {DateTime.UtcNow:o}");
            Log.Information($"[REFRESH CHECK HASH] Searching for: {refreshToken}, Found: {token?.TokenHash}");
            if (token == null) return false;
            if (token.Expires <= DateTime.UtcNow.AddSeconds(-10)) return false;
            return true;
        }
    }
}
