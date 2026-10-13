using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;
using UrlShortener.Api.Middleware;
using UrlShortener.Api.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
// The database is always stored inside the API project directory.
var dbFile = Path.Combine(builder.Environment.ContentRootPath, "shortener.db");
var dbConnection = builder.Configuration.GetConnectionString("Shortener")
    ?? $"Data Source={dbFile}";
builder.Services.AddDbContext<ShortenerDbContext>(options =>
    options.UseSqlite(dbConnection));
builder.Services.AddScoped<IUrlShortenerService, UrlShortenerService>();

var app = builder.Build();
app.UseMiddleware<ApiExceptionMiddleware>();
app.UseSwagger();
app.UseSwaggerUI();

// Prototype-only initialization. Replace with reviewed EF Core migrations in production.
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ShortenerDbContext>();
    await db.Database.EnsureCreatedAsync();
}
app.MapControllers();
app.Run();

public partial class Program { }
