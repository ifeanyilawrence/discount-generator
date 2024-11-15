namespace DiscountGenerator.Entrypoint.Models;

using Domain;

public class DiscountCodeList
{
    public DiscountCodeList(
        IEnumerable<DiscountCodeResponse> codes, 
        PageInfo pageInfo)
    {
        Codes = codes;
        PageInfo = pageInfo;
    }
    
    public IEnumerable<DiscountCodeResponse> Codes { get; set; }

    public PageInfo PageInfo { get; set; }
}

public class DiscountCodeResponse
{
    public DiscountCodeResponse(
        string code,
        bool isUsed)
    {
        Code = code;
        IsUsed = isUsed;
    }
    
    public string Code { get; set; }
    
    public bool IsUsed { get; set; }
}