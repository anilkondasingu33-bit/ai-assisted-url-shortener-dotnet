using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
public sealed class RedirectController(IUrlShortenerService service) : ControllerBase
{
    [HttpGet("/{code}")]
    public async Task<IActionResult> RedirectShortCode(string code, CancellationToken ct)
    {
        var destination = await service.ResolveAndTrackAsync(code, ct);
        return Redirect(destination); // 302 Found; response should not be permanently cached
    }
}
