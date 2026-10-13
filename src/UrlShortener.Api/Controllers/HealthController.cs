using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using UrlShortener.Api.Data;

namespace UrlShortener.Api.Controllers;

[ApiController]
public sealed class HealthController(ShortenerDbContext db) : ControllerBase
{
    [HttpGet("/health")]
    public async Task<IActionResult> Check(CancellationToken ct)
        => await db.Database.CanConnectAsync(ct)
            ? Ok(new { status = "healthy" })
            : StatusCode(503, new { status = "unhealthy" });
}
