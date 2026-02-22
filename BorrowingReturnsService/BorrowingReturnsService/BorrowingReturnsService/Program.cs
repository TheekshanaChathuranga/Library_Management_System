using BorrowingReturnsService.Data;
using BorrowingReturnsService.Repositories;
using BorrowingReturnsService.Services;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddDbContext<BorrowingReturnsDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSingleton<IConnectionMultiplexer>(ConnectionMultiplexer.Connect(builder.Configuration.GetConnectionString("Redis")));
builder.Services.AddScoped<ICacheService, RedisCacheService>();
builder.Services.AddScoped<IBorrowingRepository, BorrowingRepository>();
builder.Services.AddScoped<ILateFeeRepository, LateFeeRepository>();
builder.Services.AddScoped<ILateFeeService, LateFeeService>();

// Configure typed HTTP clients
builder.Services.AddHttpClient<ICatalogClient, CatalogClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:CatalogService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IInventoryClient, InventoryClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:InventoryService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<IUserIdentityClient, UserIdentityClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["ServiceUrls:UserIdentityService"]);
    client.Timeout = TimeSpan.FromSeconds(30);
});


builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
    });
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Apply migrations with retry logic
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var logger = services.GetRequiredService<ILogger<Program>>();
    var dbContext = services.GetRequiredService<BorrowingReturnsDbContext>();
    
    try
    {
        logger.LogInformation("Migrating database...");
        // Simple retry logic
        int retries = 5;
        while (retries > 0)
        {
            try
            {
                dbContext.Database.Migrate();
                logger.LogInformation("Database migrated successfully.");
                break;
            }
            catch (Exception ex)
            {
                retries--;
                if (retries == 0) throw;
                logger.LogWarning(ex, $"Database migration failed. Retrying in 5 seconds... ({retries} attempts remaining)");
                System.Threading.Thread.Sleep(5000);
            }
        }
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "An error occurred while migrating the database.");
        // Don't throw, let the app start so we can see the error in logs/health check
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.MapControllers();

app.Run();
