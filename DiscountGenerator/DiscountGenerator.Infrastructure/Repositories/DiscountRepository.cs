namespace DiscountGenerator.Infrastructure.Repositories;

using Application;
using DbContexts;
using Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

public class DiscountRepository : IDiscountRepository
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    
    public DiscountRepository(IServiceScopeFactory serviceScopeFactory)
    {
        _serviceScopeFactory = serviceScopeFactory;
    }
    
    public async Task<PagedResponse<DiscountCode>> GetDiscountCodesAsync(
        int pageNumber, 
        int pageSize)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
        
        var totalItems = await dbContext.DiscountCodes.AsNoTracking().CountAsync(s => !s.IsUsed);
        var totalPages = (int)Math.Ceiling(totalItems / (double)pageSize);
        
        var discountCodes = await dbContext.DiscountCodes
            .AsNoTracking()
            .Where(s => !s.IsUsed)
            .OrderBy(d => d.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(d => new DiscountCode(d.Code, d.IsUsed, d.CreatedAt, d.UsedAt))
            .ToListAsync();
        
        return new PagedResponse<DiscountCode>(
            discountCodes,
            new PageInfo(
                pageNumber: pageNumber,
                pageSize: pageSize,
                totalItems: totalItems,
                totalPages: totalPages));
    }

    public async Task<bool> UpdateCodeAsync(string code)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();

        var discountCode = await dbContext.DiscountCodes
            .FirstOrDefaultAsync(d => d.Code == code);
                
        if (discountCode is null)
        {
            throw new Exception("Discount code not found");
        }

        discountCode.IsUsed = true;
        discountCode.UsedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync();

        return true;
    }

    public async Task<bool> SaveCodesAsync(IEnumerable<DiscountCode> codes)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
        
        var discountCodes = codes.Select(c => new Entities.DiscountCode
        {
            Code = c.Code,
            IsUsed = c.IsUsed,
            CreatedAt = c.CreatedAt,
            UsedAt = c.UsedAt
        });

        try
        {
            const int batchSize = 100;
            var batchList = discountCodes.ToList();

            for (int i = 0; i < batchList.Count; i += batchSize)
            {
                var batch = batchList.Skip(i).Take(batchSize);
                await dbContext.DiscountCodes.AddRangeAsync(batch);
                await dbContext.SaveChangesAsync();
            }
            
            return true;
        }
        catch (DbUpdateException ex)
        {
            throw new Exception("Database update failed", ex);
        }
        catch (Exception ex)
        {
            throw new Exception("Error occurred while saving discount codes.", ex);
        }
    }

    public async Task<IEnumerable<DiscountCode>> LoadDiscountCodesAsync()
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
        
        var discountCodes = await dbContext.DiscountCodes
            .AsNoTracking()
            .ToListAsync();
        
        return discountCodes.Select(d => new DiscountCode(
            code: d.Code,
            isUsed: d.IsUsed,
            createdAt: d.CreatedAt,
            usedAt: d.UsedAt));
    }

    public async Task<DiscountCode?> GetDiscountCodeAsync(string code)
    {
        using var scope = _serviceScopeFactory.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
        
        var discountCode = await dbContext.DiscountCodes
            .AsNoTracking()
            .FirstOrDefaultAsync(d => d.Code == code);
        
        return discountCode is null
            ? null
            : new DiscountCode(
                code: discountCode.Code,
                isUsed: discountCode.IsUsed,
                createdAt: discountCode.CreatedAt,
                usedAt: discountCode.UsedAt);
    }
}