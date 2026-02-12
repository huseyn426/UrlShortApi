# URL Shortener API (.NET 8 + PostgreSQL)

Simple URL shortener service:
- Create short links
- Redirect by code
- Click tracking

## Tech Stack
- ASP.NET Core Web API (.NET 8)
- EF Core 8
- PostgreSQL
- Swagger

## Endpoints
- `POST /api/Links` – create short link
- `GET /api/Links` – list links
- `GET /{code}` – redirect to original URL

## Run locally
1) Configure PostgreSQL connection string:
- `appsettings.json` uses `ConnectionStrings:Default`
- Recommended: use `dotnet user-secrets`

Example:
```bash
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=url_shortener_db;Username=postgres;Password=YOUR_PASSWORD"
