using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace UrlShortener.Api.Contracts;

public sealed class CreateShortLinkRequest
{
    [Required]
    [JsonPropertyName("originalUrl")]
    public required string OriginalUrl { get; init; }

    [JsonPropertyName("customAlias")]
    public string? CustomAlias { get; init; }

    [Range(1, 365)]
    [JsonPropertyName("expiresInDays")]
    public int? ExpiresInDays { get; init; }
}
