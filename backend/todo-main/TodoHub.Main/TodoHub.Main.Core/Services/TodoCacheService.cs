
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

        // get all todos
        public async Task<List<TodoDTO>> GetAllTodosAsync(Guid UserId)
        {
            string HashKey = $"todos:{UserId}";
            var all = await _db.HashGetAllAsync(HashKey);
            return all.Select(x => JsonSerializer.Deserialize<TodoDTO>(x.Value)!)
                      .OrderBy(t => t.IsCompleted).ThenByDescending(t => t.CreatedDate)
                      .ToList();
        }

        // delete cache

        public async Task DeleteCache(Guid UserId)
        {
            string HashKey = $"todos:{UserId}";
            await _db.KeyDeleteAsync(HashKey);
        }
    }
}
