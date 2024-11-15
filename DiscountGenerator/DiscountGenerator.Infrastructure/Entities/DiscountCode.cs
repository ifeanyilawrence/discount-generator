namespace DiscountGenerator.Infrastructure.Entities;

public class DiscountCode
{
    public int Id { get; set; }
    
    public string Code { get; set; }
    
    public bool IsUsed { get; set; }
    
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public DateTime? UsedAt { get; set; }
}