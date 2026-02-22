using InventoryService.Dtos;
using InventoryService.Models;

namespace InventoryService.Services;

public interface IInventoryService
{
    Task<InventorySummaryDto?> GetAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<InventorySummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<InventorySummaryDto> CreateAsync(CreateInventoryDto request, CancellationToken cancellationToken = default);
    Task<InventorySummaryDto?> UpdateTotalsAsync(Guid bookId, UpdateInventoryDto request, CancellationToken cancellationToken = default);
    Task<InventorySummaryDto?> BorrowAsync(Guid bookId, AdjustInventoryDto request, CancellationToken cancellationToken = default);
    Task<InventorySummaryDto?> ReturnAsync(Guid bookId, AdjustInventoryDto request, CancellationToken cancellationToken = default);
    Task<IEnumerable<InventorySummaryDto>> GetBatchAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default);
}
