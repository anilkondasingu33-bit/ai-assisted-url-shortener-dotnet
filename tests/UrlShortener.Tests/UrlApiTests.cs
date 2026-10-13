using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using UrlShortener.Api.Contracts;
using Xunit;

namespace UrlShortener.Tests;

public sealed class UrlApiTests : IAsyncLifetime
{
    private readonly string _dbFile = Path.Combine(Path.GetTempPath(), $"shortener-{Guid.NewGuid():N}.db");
    private WebApplicationFactory<Program> _factory = null!;
    private HttpClient _client = null!;

    public Task InitializeAsync()
    {
        _factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
            builder.ConfigureAppConfiguration((_, config) =>
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:Shortener"] = $"Data Source={_dbFile}"
                })));
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        return Task.CompletedTask;
    }

    [Fact]
    public async Task CreateRedirectAnalyticsAndDisable_WorksEndToEnd()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/urls", new
        {
            originalUrl = "https://example.com/page", customAlias = "demo-link", expiresInDays = 7
        });
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var created = await response.Content.ReadFromJsonAsync<ShortLinkCreatedResponse>();
        Assert.Equal("demo-link", created!.Code);

        var redirect = await _client.GetAsync("/demo-link");
        Assert.Equal(HttpStatusCode.Redirect, redirect.StatusCode);
        Assert.Equal("https://example.com/page", redirect.Headers.Location!.ToString());

        var stats = await _client.GetFromJsonAsync<ShortLinkStatsResponse>("/api/v1/urls/demo-link/stats");
        Assert.Equal(1, stats!.ClickCount);
        Assert.True(stats.IsActive);

        var disable = await _client.DeleteAsync("/api/v1/urls/demo-link");
        Assert.Equal(HttpStatusCode.NoContent, disable.StatusCode);
        Assert.Equal(HttpStatusCode.Gone, (await _client.GetAsync("/demo-link")).StatusCode);
    }

    [Fact]
    public async Task DuplicateCustomAlias_ReturnsConflict()
    {
        var request = new { originalUrl = "https://example.com", customAlias = "taken123" };
        Assert.Equal(HttpStatusCode.Created, (await _client.PostAsJsonAsync("/api/v1/urls", request)).StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, (await _client.PostAsJsonAsync("/api/v1/urls", request)).StatusCode);
    }

    [Theory]
    [InlineData("ftp://example.com")]
    [InlineData("http://localhost:9000")]
    [InlineData("http://127.0.0.1")]
    [InlineData("file:///etc/passwd")]
    public async Task DisallowedUrls_ReturnBadRequest(string url)
    {
        var result = await _client.PostAsJsonAsync("/api/v1/urls", new { originalUrl = url });
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task MissingLink_ReturnsNotFound()
    {
        Assert.Equal(HttpStatusCode.NotFound, (await _client.GetAsync("/abcdoesnotexist")).StatusCode);
    }

    [Fact]
    public async Task InvalidExpiry_ReturnsBadRequest()
    {
        var result = await _client.PostAsJsonAsync("/api/v1/urls", new
        {
            originalUrl = "https://example.com", expiresInDays = 366
        });
        Assert.Equal(HttpStatusCode.BadRequest, result.StatusCode);
    }

    [Fact]
    public async Task Health_ReturnsOk()
    {
        Assert.Equal(HttpStatusCode.OK, (await _client.GetAsync("/health")).StatusCode);
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        foreach (var suffix in new[] { "", "-wal", "-shm" })
        {
            var filename = _dbFile + suffix;
            if (File.Exists(filename)) File.Delete(filename);
        }
    }
}
