namespace DiscountGenerator.Application;

using System.Collections.Concurrent;
using Domain;

public interface ICacheService
{
    void Set<T>(string key, T value);
    
    ConcurrentDictionary<string, DiscountCode> GetOrCreateDiscountCodesCache(string key);
}