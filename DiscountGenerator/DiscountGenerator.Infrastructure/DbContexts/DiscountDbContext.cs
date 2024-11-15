namespace DiscountGenerator.Infrastructure.DbContexts;

using Entities;
using Microsoft.EntityFrameworkCore;

public class DiscountDbContext : DbContext
{
    public DiscountDbContext(DbContextOptions<DiscountDbContext> options) : base(options)
    {
    }
    
    public DbSet<DiscountCode> DiscountCodes { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DiscountCode>().HasIndex(d => d.Code).IsUnique();
    }
}