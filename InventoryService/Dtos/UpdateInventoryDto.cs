using System.ComponentModel.DataAnnotations;

namespace InventoryService.Dtos;

public class UpdateInventoryDto
{
    [Range(0, int.MaxValue)]
    public int PhysicalTotal { get; set; }

    [Range(0, int.MaxValue)]
    public int DigitalTotal { get; set; }
}
