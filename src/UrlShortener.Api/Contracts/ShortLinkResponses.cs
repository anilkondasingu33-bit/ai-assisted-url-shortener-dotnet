namespace UrlShortener.Api.Contracts;

public sealed record ShortLinkCreatedResponse(
    string Code, string ShortUrl, string OriginalUrl, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc);

public sealed record ShortLinkStatsResponse(
    string Code, long ClickCount, DateTime CreatedAtUtc, DateTime? ExpiresAtUtc,
    DateTime? LastAccessedAtUtc, bool IsActive);
