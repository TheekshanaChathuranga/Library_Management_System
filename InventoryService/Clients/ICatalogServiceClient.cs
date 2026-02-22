using InventoryService.Dtos;

namespace InventoryService.Clients;

public interface ICatalogServiceClient
{
    Task<BookMetadataDto?> GetBookMetadataAsync(Guid bookId, CancellationToken cancellationToken = default);
    Task<Dictionary<Guid, BookMetadataDto>> GetBooksMetadataAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default);
    Task<bool> BookExistsAsync(Guid bookId, CancellationToken cancellationToken = default);
}
