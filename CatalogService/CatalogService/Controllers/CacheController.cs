using Microsoft.AspNetCore.Mvc;
using CatalogService.Services;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
// TODO: Add [Authorize] attribute for mTLS authentication later
public class CacheController : ControllerBase
{
    private readonly ICacheService _cacheService;
    private readonly ILogger<CacheController> _logger;

    public CacheController(ICacheService cacheService, ILogger<CacheController> logger)
    {
        _cacheService = cacheService;
        _logger = logger;
    }

    /// <summary>
    /// Clear all book-related caches
    /// </summary>
    [HttpDelete("clear")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearCache()
    {
        await _cacheService.RemoveByPatternAsync("book:*");
        await _cacheService.RemoveByPatternAsync("books:*");
        
        _logger.LogInformation("Cache cleared manually");
        return Ok(new { message = "Cache cleared successfully" });
    }

    /// <summary>
    /// Clear cache for a specific book by ID
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> ClearBookCache(Guid id)
    {
        await _cacheService.RemoveAsync($"book:{id}");
        
        _logger.LogInformation("Cache cleared for book {BookId}", id);
        return Ok(new { message = $"Cache cleared for book {id}" });
    }

    /// <summary>
    /// Check if a book is cached
    /// </summary>
    [HttpGet("{id:guid}/exists")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> CheckBookCache(Guid id)
    {
        var exists = await _cacheService.ExistsAsync($"book:{id}");
        return Ok(new { bookId = id, cached = exists });
    }
}
