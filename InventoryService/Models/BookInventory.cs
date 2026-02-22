using System.ComponentModel.DataAnnotations;

namespace InventoryService.Models;

public class BookInventory
{
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BookId { get; set; }

    [Range(0, int.MaxValue)]
    public int PhysicalTotal { get; set; }

    [Range(0, int.MaxValue)]
    public int PhysicalAvailable { get; set; }

    [Range(0, int.MaxValue)]
    public int DigitalTotal { get; set; }

    [Range(0, int.MaxValue)]
    public int DigitalAvailable { get; set; }

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<InventoryMovement> Movements { get; set; } = new List<InventoryMovement>();

    public void ApplyMovement(InventoryMovement movement)
    {
        LastUpdatedUtc = DateTime.UtcNow;
        switch (movement.Channel)
        {
            case InventoryChannel.Physical:
                if (movement.Direction == InventoryDirection.Outbound)
                {
                    PhysicalAvailable -= movement.Quantity;
                }
                else
                {
                    PhysicalAvailable += movement.Quantity;
                }
                PhysicalAvailable = Math.Clamp(PhysicalAvailable, 0, PhysicalTotal);
                break;
            case InventoryChannel.Digital:
                if (movement.Direction == InventoryDirection.Outbound)
                {
                    DigitalAvailable -= movement.Quantity;
                }
                else
                {
                    DigitalAvailable += movement.Quantity;
                }
                DigitalAvailable = Math.Clamp(DigitalAvailable, 0, DigitalTotal);
                break;
        }

        Movements.Add(movement);
    }
}
