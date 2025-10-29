
using StackExchange.Redis;
using System.Text.Json;
using TodoHub.Main.Core.DTOs.Response;
using TodoHub.Main.Core.Interfaces;

namespace TodoHub.Main.Core.Services
{
    // cache service through redis
    public class TodoCacheService : ITodoCacheService
    {
        private readonly IDatabase _db;
        public TodoCacheService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        // Add all todos
        public async Task SetTodosAsync(List<TodoDTO> todos, Guid UserId)
        {
            var entries = todos.Select(todo =>
            new HashEntry(todo.Id.ToString(), JsonSerializer.Serialize(todo)))
                .ToArray();
            string HashKey = $"todos:{UserId}";

            await _db.HashSetAsync(HashKey, entries);
            await _db.KeyExpireAsync(HashKey, TimeSpan.FromMinutes(10));
        }

        // get 10 todos and more
        public async Task<List<TodoDTO>> GetTodosAsync(Guid UserId, DateTime? lastCreated = null, Guid? lastId = null)
        {
            string HashKey = $"todos:{UserId}";
            var all = await _db.HashGetAllAsync(HashKey);

            var query = all.Select(x => JsonSerializer.Deserialize<TodoDTO>(x.Value)!);
            
            if (lastCreated != null && lastId != null)
            {
                query = query.Where(t => t.CreatedDate < lastCreated || (t.CreatedDate == lastCreated.Value && t.Id.CompareTo(lastId.Value) < 0));
            }

            query = query.OrderBy(t => t.IsCompleted)
                      .ThenByDescending(t => t.CreatedDate)
                      .ThenByDescending(t => t.Id);

            return query.Take(10).ToList();

        }

        // delete cache

        public async Task DeleteCache(Guid UserId)
        {
            string HashKey = $"todos:{UserId}";
            await _db.KeyDeleteAsync(HashKey);
        }
    }
}
