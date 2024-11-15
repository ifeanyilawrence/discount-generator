namespace DiscountGenerator.Tests;

using Domain;
using Infrastructure.DbContexts;
using Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class DiscountRepositoryTests
{
    private readonly IServiceScopeFactory _serviceScopeFactory;
    private readonly DiscountDbContext _dbContext;
    private readonly DiscountRepository _repository;

    public DiscountRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<DiscountDbContext>()
            .UseInMemoryDatabase(databaseName: "DiscountDb")
            .Options;

        _dbContext = new DiscountDbContext(options);

        _serviceScopeFactory = A.Fake<IServiceScopeFactory>();

        var fakeServiceProvider = A.Fake<IServiceProvider>();
        A.CallTo(() => fakeServiceProvider.GetService(typeof(DiscountDbContext)))
            .Returns(_dbContext);

        var fakeScope = A.Fake<IServiceScope>();
        A.CallTo(() => fakeScope.ServiceProvider).Returns(fakeServiceProvider);

        A.CallTo(() => _serviceScopeFactory.CreateScope()).Returns(fakeScope);

        _repository = new DiscountRepository(_serviceScopeFactory);
    }

    [Fact]
    public async Task GetDiscountCodesAsync_ShouldReturnPagedDiscountCodes()
    {
        // Arrange
        var discountCodes = new List<Infrastructure.Entities.DiscountCode>
        {
            new() { Code = "CODE1", IsUsed = false, CreatedAt = DateTime.UtcNow },
            new() { Code = "CODE2", IsUsed = false, CreatedAt = DateTime.UtcNow },
            new() { Code = "CODE3", IsUsed = false, CreatedAt = DateTime.UtcNow }
        };
        
        // clear the in-memory database
        _dbContext.DiscountCodes.RemoveRange(_dbContext.DiscountCodes);

        await _dbContext.DiscountCodes.AddRangeAsync(discountCodes);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDiscountCodesAsync(1, 2);

        // Assert
        result.Data.Count().Should().Be(2);
        result.PageInfo.TotalItems.Should().Be(3);
        result.PageInfo.TotalPages.Should().Be(2);
        result.PageInfo.PageNumber.Should().Be(1);
    }

    [Fact]
    public async Task UpdateCodeAsync_ShouldUpdateCode()
    {
        // Arrange
        var discountCode = new Infrastructure.Entities.DiscountCode { Code = "CODE1", IsUsed = false, CreatedAt = DateTime.UtcNow };
        await _dbContext.DiscountCodes.AddAsync(discountCode);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.UpdateCodeAsync("CODE1");

        // Assert
        result.Should().BeTrue();
        var updatedCode = await _dbContext.DiscountCodes.Where(s => s.Code == "CODE1").FirstOrDefaultAsync();
        updatedCode.IsUsed.Should().BeTrue();
    }

    [Fact]
    public async Task UpdateCodeAsync_ShouldThrowExceptionWhenCodeNotFound()
    {
        // Act
        Func<Task> action = async () => await _repository.UpdateCodeAsync("INVALID_CODE");

        // Assert
        await action.Should().ThrowAsync<Exception>().WithMessage("Discount code not found");
    }

    [Fact]
    public async Task SaveCodesAsync_ShouldSaveDiscountCodes()
    {
        // Arrange
        var codesToSave = new List<DiscountCode>
        {
            new("CODE1", false, DateTime.UtcNow, null),
            new("CODE2", false, DateTime.UtcNow, null)
        };
        
        // clear the in-memory database
        _dbContext.DiscountCodes.RemoveRange(_dbContext.DiscountCodes);

        // Act
        var result = await _repository.SaveCodesAsync(codesToSave);

        // Assert
        result.Should().BeTrue();
        var savedCodes = await _dbContext.DiscountCodes.ToListAsync();
        savedCodes.Count.Should().Be(2);
    }

    [Fact]
    public async Task GetDiscountCodeAsync_ShouldReturnDiscountCode()
    {
        // Arrange
        var discountCode = new Infrastructure.Entities.DiscountCode { Code = "CODE1", IsUsed = false, CreatedAt = DateTime.UtcNow };
        await _dbContext.DiscountCodes.AddAsync(discountCode);
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _repository.GetDiscountCodeAsync("CODE1");

        // Assert
        result.Should().NotBeNull();
        result.Code.Should().Be("CODE1");
    }

    [Fact]
    public async Task GetDiscountCodeAsync_ShouldReturnNullIfCodeNotFound()
    {
        // Act
        var result = await _repository.GetDiscountCodeAsync("INVALID_CODE");

        // Assert
        result.Should().BeNull();
    }
}