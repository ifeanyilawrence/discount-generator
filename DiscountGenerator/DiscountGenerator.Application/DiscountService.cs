namespace DiscountGenerator.Application;

using System.Collections.Concurrent;
using System.Security.Cryptography;
using Domain;

public class DiscountService : IDiscountService
{
    private readonly IDiscountRepository _discountRepository;
    private readonly ICacheService _cacheService;

    public DiscountService(
        IDiscountRepository discountRepository, 
        ICacheService cacheService)
    {
        _discountRepository = discountRepository;
        _cacheService = cacheService;
    }

    public async Task LoadDiscountCodesAsync()
    {
        var discountCodes = await _discountRepository.LoadDiscountCodesAsync();

        var codes = discountCodes.ToList();
        
        if (!codes.Any())
        {
            return;
        }
        var cache = _cacheService.GetOrCreateDiscountCodesCache("DiscountCodes");
        foreach (var discountCode in codes)
        {
            cache.AddOrUpdate(discountCode.Code, discountCode, (key, value) => discountCode);
        }
    }

    public async Task<PagedResponse<DiscountCode>> GetDiscountCodesAsync(
        int pageNumber, 
        int pageSize)
    {
        var cache = _cacheService.GetOrCreateDiscountCodesCache("DiscountCodes");
        var codes = cache.Values
            .Where(c => !c.IsUsed)
            .OrderByDescending(c => c.CreatedAt)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToList();
        
        if (!codes.Any())
        {
            var pagedResponse = await _discountRepository.GetDiscountCodesAsync(pageNumber, pageSize);
            foreach (var code in pagedResponse.Data)
            {
                cache.AddOrUpdate(code.Code, code, (key, value) => code);
            }
            
            return pagedResponse;
        }
        
        return new PagedResponse<DiscountCode>(
            codes,
            new PageInfo(
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalItems: cache.Count,
                totalPages: (int)Math.Ceiling(cache.Count / (double)pageSize)));
    }

    public async Task<bool> UpdateCodeAsync(string code)
    {
        var cache = _cacheService.GetOrCreateDiscountCodesCache("DiscountCodes");
        DiscountCode? discountCode;
        if (!cache.TryGetValue(code, out discountCode))
        {
            discountCode = await _discountRepository.GetDiscountCodeAsync(code);
            if (discountCode is null)
            {
                throw new Exception("Discount code not found");
            }
            
            cache.AddOrUpdate(discountCode.Code, discountCode, (key, value) => discountCode);
        }
        
        if (discountCode.IsUsed)
        {
            throw new Exception("Discount code already used");
        }
        
        discountCode.IsUsed = true;
        discountCode.UsedAt = DateTime.UtcNow;
        
        cache.AddOrUpdate(discountCode.Code, discountCode, (key, value) => discountCode);
        
        return await _discountRepository.UpdateCodeAsync(code);
    }

    public async Task<bool> GenerateDiscountCodesAsync(int count, byte length)
    {
        var cache = _cacheService.GetOrCreateDiscountCodesCache("DiscountCodes");
        var existingCodes = new ConcurrentDictionary<string, bool>(cache.Keys.ToDictionary(key => key, key => true));
        var discountCodes = new ConcurrentBag<DiscountCode>();
        var parallelOptions = new ParallelOptions { MaxDegreeOfParallelism = 10 };

        await Parallel.ForEachAsync(Enumerable.Range(0, count), parallelOptions, async (_, _) =>
        {
            string code;
            try
            {
                do
                {
                    code = GenerateCode(length);
                } while (!existingCodes.TryAdd(code, true));

                var discountCode = new DiscountCode(
                    code: code,
                    isUsed: false,
                    createdAt: DateTime.UtcNow,
                    usedAt: null
                );
                discountCodes.Add(discountCode);

                cache.AddOrUpdate(discountCode.Code, discountCode, (key, value) => discountCode);
            }
            catch (Exception ex)
            {
                throw new Exception("Error generating discount codes");
            }
        });

        return await _discountRepository.SaveCodesAsync(discountCodes.ToList());
    }

    private string GenerateCode(byte length)
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var bytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(bytes);
        }

        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }
}