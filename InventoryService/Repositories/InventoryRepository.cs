using InventoryService.Data;
using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Repositories;

public class InventoryRepository : IInventoryRepository
{
    private readonly InventoryDbContext _dbContext;

    public InventoryRepository(InventoryDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<BookInventory?> GetByBookIdAsync(Guid bookId, CancellationToken cancellationToken = default)
        => _dbContext.Inventories.Include(x => x.Movements)
                                 .SingleOrDefaultAsync(x => x.BookId == bookId, cancellationToken);

    public Task<BookInventory?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        => _dbContext.Inventories.Include(x => x.Movements)
                                 .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

    public async Task AddAsync(BookInventory inventory, CancellationToken cancellationToken = default)
    {
        await _dbContext.Inventories.AddAsync(inventory, cancellationToken);
    }

    public Task UpdateAsync(BookInventory inventory, CancellationToken cancellationToken = default)
    {
        // Entity is already tracked from GetByBookIdAsync, no need to call Update
        // EF Core will automatically detect changes
        return Task.CompletedTask;
    }

    public Task<List<BookInventory>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        return _dbContext.Inventories
            .OrderByDescending(x => x.LastUpdatedUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<List<BookInventory>> GetByBookIdsAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default)
    {
        return _dbContext.Inventories
            .Where(x => bookIds.Contains(x.BookId))
            .AsNoTracking()
            .ToListAsync(cancellationToken);
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        => _dbContext.SaveChangesAsync(cancellationToken);
}
