using ECommerce.Domain.Interfaces;

namespace ECommerce.Infrastructure.Caching;

public class MemoryCacheService : ICacheService
{
    private readonly Dictionary<string, (object Value, DateTime ExpiresAt)> _cache = new();

    public Task<T?> GetAsync<T>(string key, CancellationToken cancellationToken = default) where T : class
    {
        if (_cache.TryGetValue(key, out var entry) && entry.ExpiresAt > DateTime.UtcNow)
            return Task.FromResult(entry.Value as T);
        return Task.FromResult<T?>(null);
    }

    public Task SetAsync<T>(string key, T value, TimeSpan expiration, CancellationToken cancellationToken = default) where T : class
    {
        _cache[key] = (value, DateTime.UtcNow.Add(expiration));
        return Task.CompletedTask;
    }

    public Task RemoveAsync(string key, CancellationToken cancellationToken = default)
    {
        _cache.Remove(key);
        return Task.CompletedTask;
    }

    public Task<bool> ExistsAsync(string key, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_cache.ContainsKey(key));
    }
}
