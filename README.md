# URL Shortener — Simple .NET 8 Web API

This version keeps the business logic small and readable for an interview assessment.

## Quick start

1. Install .NET 8 SDK.
2. Open `UrlShortener.sln` in Visual Studio, or run:

```bash
dotnet restore UrlShortener.sln
dotnet build UrlShortener.sln
dotnet run --project src/UrlShortener.Api
```

Open `http://localhost:5080/swagger` (or the URL printed by `dotnet run`).

Run integration tests with `dotnet test UrlShortener.sln`.

## Where is the database?

After the API starts, SQLite creates this file automatically:

`src/UrlShortener.Api/shortener.db`

The code uses `builder.Environment.ContentRootPath`, so the file stays within the API project directory rather than changing with the terminal working directory.

Use **DB Browser for SQLite** (https://sqlitebrowser.org/) → **Open Database** → select `shortener.db` → **Browse Data** → `ShortLinks`.

SQL:

```sql
SELECT Id, Code, OriginalUrl, ClickCount, IsActive, CreatedAtUtc
FROM ShortLinks ORDER BY Id DESC;
```

Stop the application before editing the database; viewing is safe. The database is ignored by Git.

## Simple request lifecycle

1. `POST /api/v1/urls`: validate URL; choose short code; save `ShortLink` in SQLite.
2. `GET /{code}`: load the record, verify active/expiry, increase click count, redirect.
3. `GET /api/v1/urls/{code}/stats`: return saved analytics.
4. `DELETE /api/v1/urls/{code}`: disable the mapping (record remains in database).
5. `GET /health`: database health check.

Sample create JSON:

```json
{"originalUrl":"https://example.com/page","customAlias":"demo123","expiresInDays":7}
```

## Code map

- `Controllers/UrlsController.cs`: endpoints for creation, analytics, disable.
- `Controllers/RedirectController.cs`: redirect endpoint.
- `Services/UrlShortenerService.cs`: easy-to-follow create/read/update business logic.
- `Services/UrlValidation.cs`: URL security checks separate from the main service.
- `Data/ShortenerDbContext.cs`: database table and unique index.
- `Domain/ShortLink.cs`: SQLite entity columns.
- `Program.cs`: dependency injection and fixed database file location.
- `tests/UrlShortener.Tests`: integration tests.
- `docs/`: assignment-required architecture, scenarios, AI-use log and engineering summary.

## Assessment notes

This is a **prototype**, not a publicly deployable multi-user service. Add authenticated ownership, rate limits, stronger URL abuse protection, reviewed EF migrations and monitoring before deployment. Check the actual build, tests and code personally before submitting.
