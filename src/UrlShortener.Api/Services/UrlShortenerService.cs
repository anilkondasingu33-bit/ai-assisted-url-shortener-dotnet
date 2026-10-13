using System.Security.Cryptography;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.Sqlite;
using UrlShortener.Api.Contracts;
using UrlShortener.Api.Data;
using UrlShortener.Api.Domain;

namespace UrlShortener.Api.Services;

public sealed class UrlShortenerService : IUrlShortenerService
{
    private readonly ShortenerDbContext _db;

    public UrlShortenerService(ShortenerDbContext db)
    {
        _db = db;
    }

    public async Task<ShortLinkCreatedResponse> CreateAsync(
        CreateShortLinkRequest request, string baseUrl, CancellationToken ct)
    {
        string originalUrl = UrlValidation.ValidatePublicHttpUrl(request.OriginalUrl);
        string? alias = request.CustomAlias?.Trim();

        if (alias != null &&
            (!Regex.IsMatch(alias, "^[A-Za-z0-9_-]{3,32}$") ||
             new[] { "api", "health", "swagger", "docs" }.Contains(alias, StringComparer.OrdinalIgnoreCase)))
            throw new ApiException(400, "Invalid custom alias.");

        // Auto-generated codes are retried if another record already owns them.
        for (int attempt = 0; attempt < 5; attempt++)
        {
            string code = alias ?? GenerateCode();
            bool exists = await _db.ShortLinks.AnyAsync(x => x.Code == code, ct);
            if (exists)
            {
                if (alias != null) throw new ApiException(409, "Alias already exists.");
                continue;
            }

            var link = new ShortLink
            {
                Code = code,
                OriginalUrl = originalUrl,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = request.ExpiresInDays is int days
                    ? DateTime.UtcNow.AddDays(days) : null
            };
            _db.ShortLinks.Add(link);

            try
            {
                await _db.SaveChangesAsync(ct);
                return new ShortLinkCreatedResponse(
                    code, $"{baseUrl.TrimEnd('/')}/{code}",
                    originalUrl, link.CreatedAtUtc, link.ExpiresAtUtc);
            }
            catch (DbUpdateException ex) when
                (ex.InnerException is SqliteException { SqliteErrorCode: 19 })
            {
                // Another request created the same code at the same time.
                _db.Entry(link).State = EntityState.Detached;
                if (alias != null) throw new ApiException(409, "Alias already exists.");
            }
        }
        throw new ApiException(503, "Unable to generate a code. Please retry.");
    }

    public async Task<string> ResolveAndTrackAsync(string code, CancellationToken ct)
    {
        var link = await _db.ShortLinks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);
        if (link == null) throw new ApiException(404, "Short URL not found.");

        DateTime now = DateTime.UtcNow;
        if (!link.IsActive || (link.ExpiresAtUtc != null && link.ExpiresAtUtc <= now))
            throw new ApiException(410, "Short URL has expired or is disabled.");

        // Update in the database to avoid losing clicks from simultaneous requests.
        int rowsUpdated = await _db.ShortLinks
            .Where(x => x.Id == link.Id && x.IsActive &&
                        (x.ExpiresAtUtc == null || x.ExpiresAtUtc > now))
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(x => x.ClickCount, x => x.ClickCount + 1)
                .SetProperty(x => x.LastAccessedAtUtc, now), ct);

        if (rowsUpdated == 0) throw new ApiException(410, "Short URL has expired or is disabled.");
        return link.OriginalUrl;
    }

    public async Task<ShortLinkStatsResponse> GetStatsAsync(string code, CancellationToken ct)
    {
        var link = await _db.ShortLinks.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Code == code, ct);
        if (link == null) throw new ApiException(404, "Short URL not found.");

        return new ShortLinkStatsResponse(link.Code, link.ClickCount,
            link.CreatedAtUtc, link.ExpiresAtUtc, link.LastAccessedAtUtc, link.IsActive);
    }

    public async Task DisableAsync(string code, CancellationToken ct)
    {
        int updated = await _db.ShortLinks
            .Where(x => x.Code == code)
            .ExecuteUpdateAsync(x => x.SetProperty(link => link.IsActive, false), ct);
        if (updated == 0) throw new ApiException(404, "Short URL not found.");
    }

    private static string GenerateCode()
    {
        const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        return new string(Enumerable.Range(0, 8)
            .Select(_ => alphabet[RandomNumberGenerator.GetInt32(alphabet.Length)])
            .ToArray());
    }
}
