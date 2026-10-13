using UrlShortener.Api.Contracts;

namespace UrlShortener.Api.Services;

public interface IUrlShortenerService
{
    Task<ShortLinkCreatedResponse> CreateAsync(CreateShortLinkRequest request, string baseUrl, CancellationToken ct);
    Task<string> ResolveAndTrackAsync(string code, CancellationToken ct);
    Task<ShortLinkStatsResponse> GetStatsAsync(string code, CancellationToken ct);
    Task DisableAsync(string code, CancellationToken ct);
}
