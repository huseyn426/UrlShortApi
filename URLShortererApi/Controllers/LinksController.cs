using Microsoft.AspNetCore.Mvc;
using URLShortererApi.Dtos;
using URLShortererApi.Services;

namespace URLShortererApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LinksController : ControllerBase
    {
        private readonly ILinkService _service;

        public LinksController(ILinkService service)
        {
            _service = service;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateLinkRequest request, CancellationToken ct)
        {
            try
            {
                var created = await _service.CreateAsync(request, ct);
                return Created(created.ShortUrl, created);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetList([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? search = null, CancellationToken ct = default)
        {
            var items = await _service.GetListAsync(page, pageSize, search, ct);
            return Ok(items);
        }
    }
}
