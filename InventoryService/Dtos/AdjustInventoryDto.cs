using System.ComponentModel.DataAnnotations;
using InventoryService.Models;

namespace InventoryService.Dtos;

public class AdjustInventoryDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    [Required]
    public InventoryChannel Channel { get; set; }

    [MaxLength(256)]
    public string? Reference { get; set; }
}
