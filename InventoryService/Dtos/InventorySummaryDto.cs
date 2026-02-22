namespace InventoryService.Dtos;

public record InventorySummaryDto(
    Guid BookId,
    int PhysicalTotal,
    int PhysicalAvailable,
    int DigitalTotal,
    int DigitalAvailable,
    DateTime LastUpdatedUtc,
    BookMetadataDto? BookMetadata = null
);

public record BookMetadataDto(
    string Title,
    string? Isbn,
    string? Author,
    string? Genre
);
