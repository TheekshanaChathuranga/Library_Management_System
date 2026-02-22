using InventoryService.Dtos;
using InventoryService.Models;

namespace InventoryService.Extensions;

public static class InventoryMappings
{
    public static InventorySummaryDto ToDto(this BookInventory inventory, BookMetadataDto? bookMetadata = null)
        => new(
            inventory.BookId,
            inventory.PhysicalTotal,
            inventory.PhysicalAvailable,
            inventory.DigitalTotal,
            inventory.DigitalAvailable,
            inventory.LastUpdatedUtc,
            bookMetadata
        );
}
