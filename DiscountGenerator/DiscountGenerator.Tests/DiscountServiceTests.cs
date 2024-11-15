namespace DiscountGenerator.Tests;

using System.Collections.Concurrent;
using Application;
using Domain;
using Microsoft.Extensions.Caching.Memory;

public class DiscountServiceTests
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ICacheService _cacheService;
    private readonly DiscountService _discountService;
    private readonly CustomMemoryCache _memoryCache;

    public DiscountServiceTests()
    {
        _discountRepository = A.Fake<IDiscountRepository>();
        _memoryCache = new CustomMemoryCache();
        _cacheService = new CacheService(_memoryCache);

        _discountService = new DiscountService(_discountRepository, _cacheService);
    }

    [Fact]
    public async Task LoadDiscountCodesAsync_ShouldLoadAndCacheDiscountCodes()
    {
        // Arrange
        var discountCodes = new List<DiscountCode>
        {
            new("CODE1", false, DateTime.UtcNow, null),
            new("CODE2", false, DateTime.UtcNow, null)
        };
        
        A.CallTo(() => _discountRepository.LoadDiscountCodesAsync()).Returns(discountCodes);
        
        var fakeCache = new ConcurrentDictionary<string, DiscountCode>();
        
        _memoryCache.Set("DiscountCodes", fakeCache);  // Set the cache

        // Act
        await _discountService.LoadDiscountCodesAsync();

        // Assert
        fakeCache.Count.Should().Be(2);
        fakeCache.Should().ContainKey("CODE1");
        fakeCache.Should().ContainKey("CODE2");
    }

    [Fact]
    public async Task GetDiscountCodesAsync_ShouldReturnPagedDiscountCodesFromCache()
    {
        // Arrange
        var discountCodes = new List<DiscountCode>
        {
            new("CODE1", false, DateTime.UtcNow, null),
            new("CODE2", false, DateTime.UtcNow, null),
            new("CODE3", false, DateTime.UtcNow, null)
        };

        var fakeCache = new ConcurrentDictionary<string, DiscountCode>(
            discountCodes.ToDictionary(c => c.Code, c => c));
        
        _memoryCache.Set("DiscountCodes", fakeCache);

        // Act
        var result = await _discountService.GetDiscountCodesAsync(1, 2);

        // Assert
        result.Data.Count().Should().Be(2);
        result.PageInfo.TotalItems.Should().Be(3);
    }

    [Fact]
    public async Task UpdateCodeAsync_ShouldThrowExceptionIfDiscountCodeIsUsed()
    {
        // Arrange
        var discountCode = new DiscountCode("CODE1", true, DateTime.UtcNow, DateTime.UtcNow);
        var fakeCache = new ConcurrentDictionary<string, DiscountCode>();
        fakeCache.TryAdd("CODE1", discountCode);
        
        _memoryCache.Set("DiscountCodes", fakeCache);

        // Act
        Func<Task> action = async () => await _discountService.UpdateCodeAsync("CODE1");

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Discount code already used");
    }

    [Fact]
    public async Task UpdateCodeAsync_ShouldUpdateDiscountCodeIfValid()
    {
        // Arrange
        var discountCode = new DiscountCode("CODE1", false, DateTime.UtcNow, null);
        var fakeCache = new ConcurrentDictionary<string, DiscountCode>();
        fakeCache.TryAdd("CODE1", discountCode);
        
        _memoryCache.Set("DiscountCodes", fakeCache);
        
        A.CallTo(() => _discountRepository.UpdateCodeAsync("CODE1")).Returns(true);

        // Act
        var result = await _discountService.UpdateCodeAsync("CODE1");

        // Assert
        result.Should().BeTrue();
        discountCode.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task GenerateDiscountCodesAsync_ShouldGenerateAndCacheDiscountCodes()
    {
        // Arrange
        var discountCodes = new List<DiscountCode>
        {
            new("CODE1", false, DateTime.UtcNow, null),
            new("CODE2", false, DateTime.UtcNow, null)
        };

        var fakeCache = new ConcurrentDictionary<string, DiscountCode>();
        
        _memoryCache.Set("DiscountCodes", fakeCache);
        
        A.CallTo(() => _discountRepository.SaveCodesAsync(A<List<DiscountCode>>._)).Returns(true);

        // Act
        var result = await _discountService.GenerateDiscountCodesAsync(2, 6);

        // Assert
        result.Should().BeTrue();
        fakeCache.Count.Should().Be(2);
    }

    [Fact]
    public async Task GenerateDiscountCodesAsync_ShouldHandleCodeGenerationError()
    {
        // Arrange
        var fakeCache = new ConcurrentDictionary<string, DiscountCode>();
        
        _memoryCache.Set("DiscountCodes", fakeCache);
        A.CallTo(() => _discountRepository.SaveCodesAsync(A<List<DiscountCode>>._)).Throws<Exception>();

        // Act
        Func<Task> action = async () => await _discountService.GenerateDiscountCodesAsync(2, 6);

        // Assert
        await action.Should().ThrowAsync<Exception>();
    }
}

public class CustomMemoryCache : IMemoryCache
{
    private readonly Dictionary<object, object> _cache = new();

    public ICacheEntry CreateEntry(object key)
    {
        throw new NotImplementedException();
    }

    public bool TryGetValue(object key, out object value)
    {
        return _cache.TryGetValue(key, out value);
    }

    public void Set(object key, object value)
    {
        _cache[key] = value;
    }

    public void Remove(object key)
    {
        _cache.Remove(key);
    }

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}