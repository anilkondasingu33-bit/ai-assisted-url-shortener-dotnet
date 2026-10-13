using Microsoft.AspNetCore.Mvc;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Services;

namespace UrlShortener.Api.Controllers;

[ApiController]
[Route("api/v1/urls")]
public sealed class UrlsController(IUrlShortenerService service) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ShortLinkCreatedResponse), 201)]
    public async Task<ActionResult<ShortLinkCreatedResponse>> Create(
        [FromBody] CreateShortLinkRequest request, CancellationToken ct)
    {
        var baseUrl = $"{Request.Scheme}://{Request.Host}";
        var created = await service.CreateAsync(request, baseUrl, ct);
        return CreatedAtAction(nameof(Stats), new { code = created.Code }, created);
    }

    [HttpGet("{code}/stats")]
    public async Task<ActionResult<ShortLinkStatsResponse>> Stats(string code, CancellationToken ct)
        => Ok(await service.GetStatsAsync(code, ct));

    [HttpDelete("{code}")]
    public async Task<IActionResult> Disable(string code, CancellationToken ct)
    {
        await service.DisableAsync(code, ct);
        return NoContent();
    }
}
