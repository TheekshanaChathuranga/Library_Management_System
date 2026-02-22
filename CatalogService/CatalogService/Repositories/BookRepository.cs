using CatalogService.Data;
using CatalogService.Models;
using Microsoft.EntityFrameworkCore;

namespace CatalogService.Repositories;

public class BookRepository : IBookRepository
{
    private readonly CatalogDbContext _context;
    private readonly ILogger<BookRepository> _logger;

    public BookRepository(CatalogDbContext context, ILogger<BookRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        return await _context.Books.FindAsync(id);
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        return await _context.Books.FirstOrDefaultAsync(b => b.ISBN == isbn);
    }

    public async Task<Book> AddAsync(Book book)
    {
        _context.Books.Add(book);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Created book with ID {BookId}", book.Id);
        return book;
    }

    public async Task<Book?> UpdateAsync(Book book)
    {
        var existing = await _context.Books.FindAsync(book.Id);
        if (existing == null)
        {
            return null;
        }

        existing.Title = book.Title;
        existing.Author = book.Author;
        existing.ISBN = book.ISBN;
        existing.Genre = book.Genre;
        existing.IsAvailable = book.IsAvailable;
        existing.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        _logger.LogInformation("Updated book with ID {BookId}", book.Id);
        return existing;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var book = await _context.Books.FindAsync(id);
        if (book == null)
        {
            return false;
        }

        _context.Books.Remove(book);
        await _context.SaveChangesAsync();
        _logger.LogInformation("Deleted book with ID {BookId}", id);
        return true;
    }

    public async Task<(IEnumerable<Book> Items, int Total)> SearchAsync(
        string? searchTerm,
        string? author,
        string? genre,
        string? isbn,
        bool? isAvailable,
        int page,
        int pageSize,
        string? sortBy,
        bool descending)
    {
        var query = _context.Books.AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(searchTerm))
        {
            var searchLower = searchTerm.ToLower();
            query = query.Where(b =>
                b.Title.ToLower().Contains(searchLower) ||
                b.Author.ToLower().Contains(searchLower) ||
                b.ISBN.ToLower().Contains(searchLower) ||
                b.Genre.ToLower().Contains(searchLower));
        }

        if (!string.IsNullOrWhiteSpace(author))
        {
            query = query.Where(b => b.Author.ToLower().Contains(author.ToLower()));
        }

        if (!string.IsNullOrWhiteSpace(genre))
        {
            query = query.Where(b => b.Genre.ToLower() == genre.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(isbn))
        {
            query = query.Where(b => b.ISBN == isbn);
        }

        if (isAvailable.HasValue)
        {
            query = query.Where(b => b.IsAvailable == isAvailable.Value);
        }

        // Get total count before pagination
        var total = await query.CountAsync();

        // Apply sorting
        query = (sortBy?.ToLower()) switch
        {
            "title" => descending ? query.OrderByDescending(b => b.Title) : query.OrderBy(b => b.Title),
            "author" => descending ? query.OrderByDescending(b => b.Author) : query.OrderBy(b => b.Author),
            "isbn" => descending ? query.OrderByDescending(b => b.ISBN) : query.OrderBy(b => b.ISBN),
            "genre" => descending ? query.OrderByDescending(b => b.Genre) : query.OrderBy(b => b.Genre),
            "createdat" => descending ? query.OrderByDescending(b => b.CreatedAt) : query.OrderBy(b => b.CreatedAt),
            "availability" => descending ? query.OrderByDescending(b => b.IsAvailable) : query.OrderBy(b => b.IsAvailable),
            _ => query.OrderBy(b => b.Title) // Default sort by title
        };

        // Apply pagination
        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return (items, total);
    }
}
