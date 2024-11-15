namespace DiscountGenerator.Application;

using System.Collections.Concurrent;
using Domain;
using Microsoft.Extensions.Caching.Memory;

public class CacheService : ICacheService
{
    private readonly IMemoryCache _cache;

    public CacheService(IMemoryCache cache)
    {
        _cache = cache;
    }

    public void Set<T>(string key, T value)
    {
        var cacheEntryOptions = new MemoryCacheEntryOptions()
            .SetAbsoluteExpiration(DateTimeOffset.MaxValue);
            
        _cache.Set(key, value, cacheEntryOptions);
    }

    public ConcurrentDictionary<string, DiscountCode> GetOrCreateDiscountCodesCache(string key)
    {
        if (!_cache.TryGetValue(key, out ConcurrentDictionary<string, DiscountCode>? cache))
        {
            cache = new ConcurrentDictionary<string, DiscountCode>();
            Set(key, cache);
        }
        
        return cache;
    }
}