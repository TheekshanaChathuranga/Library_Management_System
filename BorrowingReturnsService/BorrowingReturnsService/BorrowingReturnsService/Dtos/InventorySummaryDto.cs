using System;

namespace BorrowingReturnsService.Dtos
{
    /// <summary>
    /// DTO matching InventoryService's InventorySummaryDto schema
    /// </summary>
    public class InventorySummaryDto
    {
        public Guid BookId { get; set; }
        public int PhysicalTotal { get; set; }
        public int PhysicalAvailable { get; set; }
        public int DigitalTotal { get; set; }
        public int DigitalAvailable { get; set; }
        public DateTime LastUpdatedUtc { get; set; }
        public BookMetadataDto BookMetadata { get; set; }
    }

    public class BookMetadataDto
    {
        public string Title { get; set; }
        public string ISBN { get; set; }
        public string Author { get; set; }
        public string Genre { get; set; }
    }
}
