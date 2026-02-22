using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos;

public class CreateInventoryDto
{
    [Required]
    public Guid BookId { get; set; }

    [Range(0, int.MaxValue)]
    public int PhysicalTotal { get; set; }

    [Range(0, int.MaxValue)]
    public int DigitalTotal { get; set; }
}
