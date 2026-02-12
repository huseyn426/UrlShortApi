namespace URLShortererApi.Dtos
{
    public class LinkResponse
    {
        public int Id { get; set; }
        public string Code { get; set; } = null!;
        public string ShortUrl { get; set; } = null!;
        public string OriginalUrl { get; set; } = null!;

        public DateTime CreatedAt { get; set; }
        public DateTime? ExpiresAt { get; set; }

        public long Clicks { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
}
