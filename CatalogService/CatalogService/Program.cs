using Microsoft.EntityFrameworkCore;
using CatalogService.Data;
using CatalogService.Repositories;
using CatalogService.Services;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Add PostgreSQL Database
builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("CatalogDatabase")));

// Add Redis
var redisConnection = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
{
    var configuration = ConfigurationOptions.Parse(redisConnection!, true);
    return ConnectionMultiplexer.Connect(configuration);
});

// Add Cache Service
builder.Services.AddSingleton<ICacheService, RedisCacheService>();

// Add Repositories with caching
builder.Services.AddScoped<BookRepository>();
builder.Services.AddScoped<IBookRepository>(sp =>
{
    var cacheEnabled = builder.Configuration.GetValue<bool>("CacheSettings:Enabled", true);
    if (cacheEnabled)
    {
        var bookRepository = sp.GetRequiredService<BookRepository>();
        var cacheService = sp.GetRequiredService<ICacheService>();
        var logger = sp.GetRequiredService<ILogger<CachedBookRepository>>();
        return new CachedBookRepository(bookRepository, cacheService, logger);
    }
    return sp.GetRequiredService<BookRepository>();
});

// TODO: Add mTLS authentication here later
// builder.Services.AddAuthentication(...)

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// TODO: Add authentication middleware for mTLS here later
// app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CatalogDbContext>();
        await context.Database.EnsureCreatedAsync();
        await DbInitializer.SeedDataAsync(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
    }
}

app.Run();
