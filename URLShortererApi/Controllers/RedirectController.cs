using Microsoft.AspNetCore.Mvc;
using URLShortererApi.Services;

namespace URLShortererApi.Controllers
{
    public class RedirectController : ControllerBase
    {
        private readonly ILinkService _service;

        public RedirectController(ILinkService service)
        {
            _service = service;
        }

        [HttpGet("/{code}")]
        public async Task<IActionResult> RedirectToOriginal([FromRoute] string code, CancellationToken ct)
        {
            var (url, exists) = await _service.ResolveAsync(code, ct);
            if (!exists) return NotFound(new { message = "Link not found or expired." });

            return Redirect(url); // 302
        }

    }
}
