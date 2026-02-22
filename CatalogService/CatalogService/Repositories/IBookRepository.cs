using CatalogService.Models;

namespace CatalogService.Repositories;

public interface IBookRepository
{
    Task<Book?> GetByIdAsync(Guid id);
    Task<Book?> GetByISBNAsync(string isbn);
    Task<Book> AddAsync(Book book);
    Task<Book?> UpdateAsync(Book book);
    Task<bool> DeleteAsync(Guid id);
    Task<(IEnumerable<Book> Items, int Total)> SearchAsync(
        string? searchTerm,
        string? author,
        string? genre,
        string? isbn,
        bool? isAvailable,
        int page,
        int pageSize,
        string? sortBy,
        bool descending);
}
