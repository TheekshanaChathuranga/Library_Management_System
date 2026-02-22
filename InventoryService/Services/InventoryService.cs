using InventoryService.Clients;
using InventoryService.Dtos;
using InventoryService.Extensions;
using InventoryService.Models;
using InventoryService.Repositories;

namespace InventoryService.Services;

public class InventoryService : IInventoryService
{
    private readonly IInventoryRepository _repository;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<InventoryService> _logger;

    public InventoryService(IInventoryRepository repository, ICatalogServiceClient catalogClient, ILogger<InventoryService> logger)
    {
        _repository = repository;
        _catalogClient = catalogClient;
        _logger = logger;
    }

    public async Task<InventorySummaryDto?> GetAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var entity = await _repository.GetByBookIdAsync(bookId, cancellationToken);
        if (entity == null)
        {
            return null;
        }

        var bookMetadata = await _catalogClient.GetBookMetadataAsync(bookId, cancellationToken);
        return entity.ToDto(bookMetadata);
    }

    public async Task<IReadOnlyCollection<InventorySummaryDto>> GetPagedAsync(int page, int pageSize, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetPagedAsync(page, pageSize, cancellationToken);
        var bookIds = entities.Select(x => x.BookId).ToList();
        
        var booksMetadata = await _catalogClient.GetBooksMetadataAsync(bookIds, cancellationToken);
        
        return entities.Select(x => x.ToDto(booksMetadata.GetValueOrDefault(x.BookId))).ToArray();
    }

    public async Task<IEnumerable<InventorySummaryDto>> GetBatchAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default)
    {
        var entities = await _repository.GetByBookIdsAsync(bookIds, cancellationToken);
        var foundBookIds = entities.Select(x => x.BookId).ToList();
        
        var booksMetadata = await _catalogClient.GetBooksMetadataAsync(foundBookIds, cancellationToken);
        
        return entities.Select(x => x.ToDto(booksMetadata.GetValueOrDefault(x.BookId))).ToArray();
    }

    public async Task<InventorySummaryDto> CreateAsync(CreateInventoryDto request, CancellationToken cancellationToken = default)
    {
        // Validate book exists in CatalogService
        var bookExists = await _catalogClient.BookExistsAsync(request.BookId, cancellationToken);
        if (!bookExists)
        {
            throw new InvalidOperationException($"Book with ID {request.BookId} does not exist in CatalogService");
        }

        var existing = await _repository.GetByBookIdAsync(request.BookId, cancellationToken);
        if (existing is not null)
        {
            throw new InvalidOperationException($"Inventory already exists for book {request.BookId}");
        }

        var inventory = new BookInventory
        {
            BookId = request.BookId,
            PhysicalTotal = request.PhysicalTotal,
            DigitalTotal = request.DigitalTotal,
            PhysicalAvailable = request.PhysicalTotal,
            DigitalAvailable = request.DigitalTotal,
        };

        await _repository.AddAsync(inventory, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory created for BookId {BookId}", request.BookId);
        
        var bookMetadata = await _catalogClient.GetBookMetadataAsync(request.BookId, cancellationToken);
        return inventory.ToDto(bookMetadata);
    }

    public async Task<InventorySummaryDto?> UpdateTotalsAsync(Guid bookId, UpdateInventoryDto request, CancellationToken cancellationToken = default)
    {
        var inventory = await _repository.GetByBookIdAsync(bookId, cancellationToken);
        if (inventory is null)
        {
            return null;
        }

        AdjustTotals(inventory, request.PhysicalTotal, request.DigitalTotal);
        await _repository.UpdateAsync(inventory, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        var bookMetadata = await _catalogClient.GetBookMetadataAsync(bookId, cancellationToken);
        return inventory.ToDto(bookMetadata);
    }

    public Task<InventorySummaryDto?> BorrowAsync(Guid bookId, AdjustInventoryDto request, CancellationToken cancellationToken = default)
        => AdjustAvailabilityAsync(bookId, request, InventoryDirection.Outbound, cancellationToken);

    public Task<InventorySummaryDto?> ReturnAsync(Guid bookId, AdjustInventoryDto request, CancellationToken cancellationToken = default)
        => AdjustAvailabilityAsync(bookId, request, InventoryDirection.Inbound, cancellationToken);

    private async Task<InventorySummaryDto?> AdjustAvailabilityAsync(Guid bookId, AdjustInventoryDto request, InventoryDirection direction, CancellationToken cancellationToken)
    {
        var inventory = await _repository.GetByBookIdAsync(bookId, cancellationToken);
        if (inventory is null)
        {
            return null;
        }

        _logger.LogInformation("AdjustAvailabilityAsync: BookId {BookId}, Channel {Channel}, Direction {Direction}, Qty {Qty}. Current: P={P}/D={D}", 
            bookId, request.Channel, direction, request.Quantity, inventory.PhysicalAvailable, inventory.DigitalAvailable);

        if (!CanMove(inventory, request.Channel, direction, request.Quantity))
        {
            _logger.LogWarning("AdjustAvailabilityAsync: Cannot move. P={P}, D={D}, Req={Req}", inventory.PhysicalAvailable, inventory.DigitalAvailable, request.Quantity);
            throw new InvalidOperationException("Requested quantity exceeds availability or totals");
        }

        var movement = new InventoryMovement
        {
            BookInventoryId = inventory.Id,
            Channel = request.Channel,
            Direction = direction,
            Quantity = request.Quantity,
            Reference = request.Reference
        };

        inventory.ApplyMovement(movement);
        
        _logger.LogInformation("AdjustAvailabilityAsync: Applied movement. New: P={P}/D={D}", inventory.PhysicalAvailable, inventory.DigitalAvailable);

        await _repository.UpdateAsync(inventory, cancellationToken);
        await _repository.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Inventory adjusted and saved for BookId {BookId}", bookId);
        
        var bookMetadata = await _catalogClient.GetBookMetadataAsync(bookId, cancellationToken);
        return inventory.ToDto(bookMetadata);
    }

    private static bool CanMove(BookInventory inventory, InventoryChannel channel, InventoryDirection direction, int quantity)
    {
        return channel switch
        {
            InventoryChannel.Physical when direction == InventoryDirection.Outbound => inventory.PhysicalAvailable >= quantity,
            InventoryChannel.Physical => inventory.PhysicalAvailable + quantity <= inventory.PhysicalTotal,
            InventoryChannel.Digital when direction == InventoryDirection.Outbound => inventory.DigitalAvailable >= quantity,
            InventoryChannel.Digital => inventory.DigitalAvailable + quantity <= inventory.DigitalTotal,
            _ => false
        };
    }

    private static void AdjustTotals(BookInventory inventory, int physicalTotal, int digitalTotal)
    {
        inventory.PhysicalTotal = physicalTotal;
        inventory.DigitalTotal = digitalTotal;
        inventory.PhysicalAvailable = Math.Min(inventory.PhysicalAvailable, physicalTotal);
        inventory.DigitalAvailable = Math.Min(inventory.DigitalAvailable, digitalTotal);
        inventory.LastUpdatedUtc = DateTime.UtcNow;
    }
}
