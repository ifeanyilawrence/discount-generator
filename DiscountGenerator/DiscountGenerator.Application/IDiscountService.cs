namespace DiscountGenerator.Application;

using Domain;

public interface IDiscountService
{
    Task LoadDiscountCodesAsync();
    
    Task<PagedResponse<DiscountCode>> GetDiscountCodesAsync(int pageNumber, int pageSize);
    
    Task<bool> UpdateCodeAsync(string code);
    
    Task<bool> GenerateDiscountCodesAsync(int count, byte length);
}