using System.ComponentModel.DataAnnotations;

namespace URLShortererApi.Dtos
{
    public class CreateLinkRequest
    {
        [Required]
        [MaxLength(2048)]
        public string OriginalUrl { get; set; } = null!;

        // опционально: пользовательский код
        [MaxLength(32)]
        public string? CustomCode { get; set; }

        // опционально: срок жизни
        public DateTime? ExpiresAt { get; set; }
    }
}
