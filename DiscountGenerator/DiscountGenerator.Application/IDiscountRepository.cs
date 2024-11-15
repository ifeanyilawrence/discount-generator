namespace DiscountGenerator.Application;

using Domain;

public interface IDiscountRepository
{
    Task<PagedResponse<DiscountCode>> GetDiscountCodesAsync(int pageNumber, int pageSize);
    
    Task<bool> UpdateCodeAsync(string code);
    
    Task<bool> SaveCodesAsync(IEnumerable<DiscountCode> codes);
    
    Task<IEnumerable<DiscountCode>> LoadDiscountCodesAsync();
    
    Task<DiscountCode?> GetDiscountCodeAsync(string code);
}