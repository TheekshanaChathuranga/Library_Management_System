namespace BorrowingReturnsService.Dtos
{
    /// <summary>
    /// DTO matching InventoryService's AdjustInventoryDto schema
    /// </summary>
    public class AdjustInventoryDto
    {
        public int Quantity { get; set; }
        public int Channel { get; set; } // 0 = Physical, 1 = Digital
        public string Reference { get; set; }
    }
}
