using URLShortererApi.Dtos;

namespace URLShortererApi.Services
{
    public interface ILinkService
    {
        Task<LinkResponse> CreateAsync(CreateLinkRequest request, CancellationToken ct = default);
        Task<(string originalUrl, bool exists)> ResolveAsync(string code, CancellationToken ct = default);
        Task<List<LinkResponse>> GetListAsync(int page, int pageSize, string? search, CancellationToken ct = default);
    }
}
