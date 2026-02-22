using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models;

public class InventoryMovement
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid BookInventoryId { get; set; }

    public BookInventory? Inventory { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public InventoryDirection Direction { get; set; }

    [Required]
    public InventoryChannel Channel { get; set; }

    [MaxLength(256)]
    public string? Reference { get; set; }

    public DateTime TimestampUtc { get; set; } = DateTime.UtcNow;
}

public enum InventoryDirection
{
    Outbound = -1,
    Inbound = 1
}

public enum InventoryChannel
{
    Physical = 0,
    Digital = 1
}
