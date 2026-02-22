using CatalogService.Models;
using CatalogService.Repositories;

namespace CatalogService.Services;

public class CachedBookRepository : IBookRepository
{
    private readonly IBookRepository _bookRepository;
    private readonly ICacheService _cacheService;
    private readonly ILogger<CachedBookRepository> _logger;
    
    private const string BookCacheKeyPrefix = "book:";
    private const string BookIsbnCacheKeyPrefix = "book:isbn:";
    private const string SearchCacheKeyPrefix = "books:search:";
    private static readonly TimeSpan CacheExpiration = TimeSpan.FromMinutes(30);

    public CachedBookRepository(
        IBookRepository bookRepository,
        ICacheService cacheService,
        ILogger<CachedBookRepository> logger)
    {
        _bookRepository = bookRepository;
        _cacheService = cacheService;
        _logger = logger;
    }

    public async Task<Book?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"{BookCacheKeyPrefix}{id}";
        
        // Try to get from cache
        var cachedBook = await _cacheService.GetAsync<Book>(cacheKey);
        if (cachedBook != null)
        {
            _logger.LogInformation("Retrieved book {BookId} from cache", id);
            return cachedBook;
        }

        // Get from database
        var book = await _bookRepository.GetByIdAsync(id);
        if (book != null)
        {
            // Cache the result
            await _cacheService.SetAsync(cacheKey, book, CacheExpiration);
            _logger.LogInformation("Cached book {BookId}", id);
        }

        return book;
    }

    public async Task<Book?> GetByISBNAsync(string isbn)
    {
        var cacheKey = $"{BookIsbnCacheKeyPrefix}{isbn}";
        
        // Try to get from cache
        var cachedBook = await _cacheService.GetAsync<Book>(cacheKey);
        if (cachedBook != null)
        {
            _logger.LogInformation("Retrieved book with ISBN {ISBN} from cache", isbn);
            return cachedBook;
        }

        // Get from database
        var book = await _bookRepository.GetByISBNAsync(isbn);
        if (book != null)
        {
            // Cache the result (both by ID and ISBN)
            await _cacheService.SetAsync(cacheKey, book, CacheExpiration);
            await _cacheService.SetAsync($"{BookCacheKeyPrefix}{book.Id}", book, CacheExpiration);
            _logger.LogInformation("Cached book with ISBN {ISBN}", isbn);
        }

        return book;
    }

    public async Task<Book> AddAsync(Book book)
    {
        var createdBook = await _bookRepository.AddAsync(book);
        
        // Cache the newly created book
        await _cacheService.SetAsync($"{BookCacheKeyPrefix}{createdBook.Id}", createdBook, CacheExpiration);
        await _cacheService.SetAsync($"{BookIsbnCacheKeyPrefix}{createdBook.ISBN}", createdBook, CacheExpiration);
        
        // Invalidate search cache
        await _cacheService.RemoveByPatternAsync($"{SearchCacheKeyPrefix}*");
        
        _logger.LogInformation("Added and cached new book {BookId}", createdBook.Id);
        return createdBook;
    }

    public async Task<Book?> UpdateAsync(Book book)
    {
        var updatedBook = await _bookRepository.UpdateAsync(book);
        
        if (updatedBook != null)
        {
            // Update cache
            await _cacheService.SetAsync($"{BookCacheKeyPrefix}{updatedBook.Id}", updatedBook, CacheExpiration);
            await _cacheService.SetAsync($"{BookIsbnCacheKeyPrefix}{updatedBook.ISBN}", updatedBook, CacheExpiration);
            
            // Invalidate search cache
            await _cacheService.RemoveByPatternAsync($"{SearchCacheKeyPrefix}*");
            
            _logger.LogInformation("Updated and cached book {BookId}", updatedBook.Id);
        }

        return updatedBook;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        // Get book first to know the ISBN
        var book = await _bookRepository.GetByIdAsync(id);
        
        var success = await _bookRepository.DeleteAsync(id);
        
        if (success && book != null)
        {
            // Remove from cache
            await _cacheService.RemoveAsync($"{BookCacheKeyPrefix}{id}");
            await _cacheService.RemoveAsync($"{BookIsbnCacheKeyPrefix}{book.ISBN}");
            
            // Invalidate search cache
            await _cacheService.RemoveByPatternAsync($"{SearchCacheKeyPrefix}*");
            
            _logger.LogInformation("Deleted and removed book {BookId} from cache", id);
        }

        return success;
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
        // Create cache key based on search parameters
        var cacheKey = $"{SearchCacheKeyPrefix}{searchTerm}:{author}:{genre}:{isbn}:{isAvailable}:{page}:{pageSize}:{sortBy}:{descending}";
        
        // Try to get from cache
        var cachedResult = await _cacheService.GetAsync<SearchResult>(cacheKey);
        if (cachedResult != null)
        {
            _logger.LogInformation("Retrieved search results from cache");
            return (cachedResult.Items, cachedResult.Total);
        }

        // Get from database
        var (items, total) = await _bookRepository.SearchAsync(
            searchTerm, author, genre, isbn, isAvailable, page, pageSize, sortBy, descending);

        // Cache the result (shorter expiration for search results)
        var searchResult = new SearchResult { Items = items.ToList(), Total = total };
        await _cacheService.SetAsync(cacheKey, searchResult, TimeSpan.FromMinutes(10));
        
        _logger.LogInformation("Cached search results");
        return (items, total);
    }

    // Helper class for caching search results
    private class SearchResult
    {
        public List<Book> Items { get; set; } = new();
        public int Total { get; set; }
    }
}
