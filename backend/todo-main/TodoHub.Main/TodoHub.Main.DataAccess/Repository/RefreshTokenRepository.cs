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
        public async Task<bool> AddRefreshTokenRepo(string refreshToken, Guid userId, CancellationToken ct)
        {
            Log.Information("AddRefreshTokenRepo starting in RTR");
            var entity = new RefreshTokenEntity
            {
                UserId = userId,
                TokenHash = refreshToken
            };
            await _context.RefreshTokens.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // delete old tokens
        public async Task<bool> DeleteOldTokensRepo(CancellationToken ct)
        {
            Log.Information("DeleteOldTokenRepo starting in RTR");

            var tomorrow = DateTime.UtcNow.AddDays(1);
            var oldTokens = await _context.RefreshTokens.Where(token => token.Expires < tomorrow || token.Revoked != null).ToListAsync(ct);
            _context.RemoveRange(oldTokens);
            await _context.SaveChangesAsync(ct);
            Log.Information("Old tokens deleted");
            return true;
        }

        // get token
        public async Task<RefreshTokenEntity?> GetTokenRepo(string refreshToken, CancellationToken ct)
        {
            Log.Information("GetTokenRepo starting in RTR");

            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken, ct);
            if (res == null) return null;
            return res;
        }

        // get User id by refresh token
        public async Task<Guid?> GetUserIdRepo(string hash_token, CancellationToken ct)
        {
            Log.Information("GetUserIdRepo starting in RTR");
            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == hash_token, ct);
            return res?.UserId;
        }

        // refresh old token
        public async Task<bool> RefreshTokenRepo(string refreshToken, string newToken, Guid userId, CancellationToken ct)
        {
            Log.Information("RefreshTokenRepo starting in RTR");

            // revoke old token
            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash.Equals(refreshToken), ct);
            if (token == null)
            {
                return false;
            }
            while (token!.ReplacedByToken != null)
            {
                token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == token.ReplacedByToken, ct);
            }
            token.Revoked = DateTime.UtcNow;
            token.ReplacedByToken = newToken;
            // add new token
            var entity = new RefreshTokenEntity
            {
                UserId = userId,
                TokenHash = newToken
            };
            await _context.RefreshTokens.AddAsync(entity, ct);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // revoke token
        public async Task<bool> RevokeRefreshTokenRepo(string refreshToken, CancellationToken ct)
        {
            Log.Information("RevokeRefreshTokenRepo starting in RTR");

            var res = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken, ct);
            if (res == null)
            {
                return false;
            }
            res.Revoked = DateTime.UtcNow;
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // delete refresh tokens by user
        public async Task<bool> DeleteRefreshTokensByUserRepo(Guid userId, CancellationToken ct)
        {
            Log.Information("DeleteRefreshTokensByUserRepo starting in RTR");

            var refreshTokens = await _context.RefreshTokens.Where(t => t.UserId == userId).ToListAsync(ct);
            _context.RemoveRange(refreshTokens);
            await _context.SaveChangesAsync(ct);
            return true;
        }

        // is Refresh Token valid?
        public async Task<bool> isRefreshTokenValidRepo(string refreshToken, CancellationToken ct)
        {
            Log.Information("isRefreshTokenValidRepo starting in RTR");

            var token = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.TokenHash == refreshToken, ct);

            while (token?.ReplacedByToken != null)
            {
                token = await _context.RefreshTokens
                    .FirstOrDefaultAsync(t => t.TokenHash == token.ReplacedByToken, ct);
            }

            return token != null
                && token.ReplacedByToken == null
                && token.Expires > DateTime.UtcNow.AddSeconds(-10);
        }
    }
}
