namespace UrlShortener.Api.Domain;

public sealed class ShortLink
{
    public long Id { get; set; }
    public required string Code { get; set; }
    public required string OriginalUrl { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
    public bool IsActive { get; set; } = true;
    public long ClickCount { get; set; }
    public DateTime? LastAccessedAtUtc { get; set; }
}
