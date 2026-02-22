using System.ComponentModel.DataAnnotations;

namespace CatalogService.Dtos;

public class UpdateBookDto
{
    [Required(ErrorMessage = "Title is required")]
    [StringLength(500, ErrorMessage = "Title cannot exceed 500 characters")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Author is required")]
    [StringLength(200, ErrorMessage = "Author name cannot exceed 200 characters")]
    public string Author { get; set; } = null!;

    [Required(ErrorMessage = "ISBN is required")]
    [StringLength(20, ErrorMessage = "ISBN cannot exceed 20 characters")]
    [RegularExpression(@"^(?=(?:\D*\d){10}(?:(?:\D*\d){3})?$)[\d-]+$", ErrorMessage = "Invalid ISBN format")]
    public string ISBN { get; set; } = null!;

    [Required(ErrorMessage = "Genre is required")]
    [StringLength(100, ErrorMessage = "Genre cannot exceed 100 characters")]
    public string Genre { get; set; } = null!;

    public bool IsAvailable { get; set; }
}
