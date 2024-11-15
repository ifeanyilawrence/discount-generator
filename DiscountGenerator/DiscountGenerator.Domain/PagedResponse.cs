namespace DiscountGenerator.Domain;

public class PagedResponse<T>
{
    public PagedResponse(
        IEnumerable<T> data, 
        PageInfo pageInfo)
    {
        Data = data;
        PageInfo = pageInfo;
    }

    public IEnumerable<T> Data { get; set; }

    public PageInfo PageInfo { get; set; }
}