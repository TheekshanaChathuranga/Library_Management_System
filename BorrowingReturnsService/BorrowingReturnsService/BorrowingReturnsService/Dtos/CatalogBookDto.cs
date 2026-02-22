using System;

namespace BorrowingReturnsService.Dtos
{
    /// <summary>
    /// DTO matching CatalogService's BookDto schema
    /// </summary>
    public class CatalogBookDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public string ISBN { get; set; }
        public string Genre { get; set; }
        public bool IsAvailable { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
