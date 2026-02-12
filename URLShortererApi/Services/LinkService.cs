using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using URLShortererApi.Data;
using URLShortererApi.Dtos;
using URLShortererApi.Entities;

namespace URLShortererApi.Services
{
    public class LinkService:ILinkService
    {
        private readonly AppDbContext _db;
        private readonly string _baseUrl;

        private static readonly Regex CodeRegex = new(@"^[a-zA-Z0-9_-]{4,32}$", RegexOptions.Compiled);

        public LinkService(AppDbContext db, IConfiguration config)
        {
            _db = db;
            _baseUrl = config["App:BaseUrl"]?.TrimEnd('/') ?? "https://localhost:5001";
        }

        public async Task<LinkResponse> CreateAsync(CreateLinkRequest request, CancellationToken ct = default)
        {
            // 1) Валидация URL
            if (!Uri.TryCreate(request.OriginalUrl, UriKind.Absolute, out var uri) ||
                (uri.Scheme != Uri.UriSchemeHttp && uri.Scheme != Uri.UriSchemeHttps))
            {
                throw new ArgumentException("OriginalUrl must be a valid absolute http/https URL.");
            }

            // 2) ExpiresAt в будущем (если задан)
            if (request.ExpiresAt.HasValue && request.ExpiresAt.Value <= DateTime.UtcNow)
            {
                throw new ArgumentException("ExpiresAt must be in the future.");
            }

            // 3) Код: custom или генерация
            string code;
            if (!string.IsNullOrWhiteSpace(request.CustomCode))
            {
                code = request.CustomCode.Trim();

                if (!CodeRegex.IsMatch(code))
                    throw new ArgumentException("CustomCode must match ^[a-zA-Z0-9_-]{4,32}$");

                var exists = await _db.ShortLinks.AnyAsync(x => x.Code == code, ct);
                if (exists)
                    throw new ArgumentException("CustomCode is already taken.");
            }
            else
            {
                code = await GenerateUniqueCodeAsync(ct);
            }

            var entity = new ShortLink
            {
                Code = code,
                OriginalUrl = request.OriginalUrl.Trim(),
                CreatedAt = DateTime.UtcNow,
                ExpiresAt = request.ExpiresAt,
                Clicks = 0
            };

            _db.ShortLinks.Add(entity);
            await _db.SaveChangesAsync(ct);

            return ToResponse(entity);
        }

        public async Task<(string originalUrl, bool exists)> ResolveAsync(string code, CancellationToken ct = default)
        {
            code = code.Trim();

            var link = await _db.ShortLinks.FirstOrDefaultAsync(x => x.Code == code, ct);
            if (link is null) return (string.Empty, false);

            if (link.ExpiresAt.HasValue && link.ExpiresAt.Value <= DateTime.UtcNow)
                return (string.Empty, false);

            // обновляем статистику
            link.Clicks += 1;
            link.LastAccessedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            return (link.OriginalUrl, true);
        }

        public async Task<List<LinkResponse>> GetListAsync(int page, int pageSize, string? search, CancellationToken ct = default)
        {
            if (page < 1) page = 1;
            if (pageSize < 1) pageSize = 20;
            if (pageSize > 100) pageSize = 100;

            var q = _db.ShortLinks.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                q = q.Where(x => x.Code.Contains(search) || x.OriginalUrl.Contains(search));
            }

            var items = await q
                .OrderByDescending(x => x.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync(ct);

            return items.Select(ToResponse).ToList();
        }

        private LinkResponse ToResponse(ShortLink link)
        {
            return new LinkResponse
            {
                Id = link.Id,
                Code = link.Code,
                ShortUrl = $"{_baseUrl}/{link.Code}",
                OriginalUrl = link.OriginalUrl,
                CreatedAt = link.CreatedAt,
                ExpiresAt = link.ExpiresAt,
                Clicks = link.Clicks,
                LastAccessedAt = link.LastAccessedAt
            };
        }

        private async Task<string> GenerateUniqueCodeAsync(CancellationToken ct)
        {
            // 7 символов: достаточно для старта
            const string alphabet = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            for (int attempt = 0; attempt < 10; attempt++)
            {
                var code = RandomCode(alphabet, 7);

                var exists = await _db.ShortLinks.AnyAsync(x => x.Code == code, ct);
                if (!exists) return code;
            }

            throw new Exception("Failed to generate a unique code. Try again.");
        }

        private static string RandomCode(string alphabet, int length)
        {
            var chars = new char[length];
            for (int i = 0; i < length; i++)
            {
                chars[i] = alphabet[Random.Shared.Next(alphabet.Length)];
            }
            return new string(chars);
        }

    }
}
