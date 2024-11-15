namespace DiscountGenerator.Domain;

public class DiscountCode
{
    public DiscountCode(
        string code, 
        bool isUsed, 
        DateTime createdAt, 
        DateTime? usedAt)
    {
        Code = code;
        IsUsed = isUsed;
        CreatedAt = createdAt;
        UsedAt = usedAt;
    }
    
    public string Code { get; set; }
    
    public bool IsUsed { get; set; }
    
    public DateTime CreatedAt { get; set; }
    
    public DateTime? UsedAt { get; set; }
}