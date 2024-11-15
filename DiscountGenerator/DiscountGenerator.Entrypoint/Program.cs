using DiscountGenerator.Application;
using DiscountGenerator.Entrypoint.Hub;
using DiscountGenerator.Infrastructure.DbContexts;
using DiscountGenerator.Infrastructure.Repositories;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json");

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<DiscountDbContext>(x => x.UseSqlite(connectionString));

// Enable CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReactApp", policy =>
    {
        policy.WithOrigins("http://localhost:3000") // Your React app's URL
            .AllowAnyMethod()  // Allow all methods (GET, POST, etc.)
            .AllowAnyHeader()  // Allow all headers
            .AllowCredentials(); // Allow credentials (cookies, authentication)
    });
});

builder.Services.AddSignalR();
builder.Services.AddMemoryCache();

builder.Services.AddSingleton<ICacheService, CacheService>();
builder.Services.AddTransient<IDiscountService, DiscountService>();
builder.Services.AddTransient<IDiscountRepository, DiscountRepository>();

var app = builder.Build();

app.UseCors("AllowReactApp"); 

await RunMigrationsAsync(app.Services);
await LoadDiscountCodesAsync(app.Services);

app.UseRouting();
app.MapHub<DiscountHub>("/discount-hub");

app.Run();


static async Task RunMigrationsAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<DiscountDbContext>();
    await dbContext.Database.MigrateAsync();
}

static async Task LoadDiscountCodesAsync(IServiceProvider serviceProvider)
{
    using var scope = serviceProvider.CreateScope();
    var discountService = scope.ServiceProvider.GetRequiredService<IDiscountService>();
    await discountService.LoadDiscountCodesAsync();
}