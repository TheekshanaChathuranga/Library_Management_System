using InventoryService.Models;

namespace InventoryService.Repositories;

public interface IInventoryRepository
{
    Task<BookInventory?> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<BookInventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task AddAsync(BookInventory inventory, CancellationToken cancellationToken = default);
    Task UpdateAsync(BookInventory inventory, CancellationToken cancellationToken = default);
    Task<List<BookInventory>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default);
    Task<List<BookInventory>> GetByBookIdsAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default);
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
