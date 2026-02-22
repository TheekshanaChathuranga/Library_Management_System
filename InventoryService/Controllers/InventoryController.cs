using InventoryService.Dtos;
using InventoryService.Models;
using InventoryService.Services;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InventoryController : ControllerBase
{
    private readonly IInventoryService _inventoryService;

    public InventoryController(IInventoryService inventoryService)
    {
        _inventoryService = inventoryService;
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<InventorySummaryDto>>> GetInventories([FromQuery] int page = 1, [FromQuery] int pageSize = 25, CancellationToken cancellationToken = default)
    {
        var result = await _inventoryService.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(result);
    }

    [HttpGet("{bookId:guid}")]
    public async Task<ActionResult<InventorySummaryDto>> GetInventory(Guid bookId, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetAsync(bookId, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("batch")]
    public async Task<ActionResult<IEnumerable<InventorySummaryDto>>> GetBatch([FromBody] IEnumerable<Guid> bookIds, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.GetBatchAsync(bookIds, cancellationToken);
        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<InventorySummaryDto>> CreateInventory([FromBody] CreateInventoryDto request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.CreateAsync(request, cancellationToken);
        return CreatedAtAction(nameof(GetInventory), new { bookId = result.BookId }, result);
    }

    [HttpPut("{bookId:guid}")]
    public async Task<ActionResult<InventorySummaryDto>> UpdateTotals(Guid bookId, [FromBody] UpdateInventoryDto request, CancellationToken cancellationToken)
    {
        var result = await _inventoryService.UpdateTotalsAsync(bookId, request, cancellationToken);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("{bookId:guid}/borrow")]
    public async Task<ActionResult<InventorySummaryDto>> Borrow(Guid bookId, [FromBody] AdjustInventoryDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.BorrowAsync(bookId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to borrow",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }

    [HttpPost("{bookId:guid}/return")]
    public async Task<ActionResult<InventorySummaryDto>> Return(Guid bookId, [FromBody] AdjustInventoryDto request, CancellationToken cancellationToken)
    {
        try
        {
            var result = await _inventoryService.ReturnAsync(bookId, request, cancellationToken);
            return result is null ? NotFound() : Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Unable to return",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
