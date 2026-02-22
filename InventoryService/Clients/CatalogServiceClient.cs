using System.Net;
using System.Text.Json;
using InventoryService.Dtos;

namespace InventoryService.Clients;

public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogServiceClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;

    public CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<BookMetadataDto?> GetBookMetadataAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/books/{bookId}", cancellationToken);
            
            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }

            response.EnsureSuccessStatusCode();

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var bookDto = JsonSerializer.Deserialize<CatalogBookDto>(content, _jsonOptions);

            if (bookDto == null)
            {
                return null;
            }

            return new BookMetadataDto(
                bookDto.Title ?? string.Empty,
                bookDto.Isbn,
                bookDto.Author,
                bookDto.Genre
            );
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to fetch book metadata for BookId {BookId} from CatalogService", bookId);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error fetching book metadata for BookId {BookId}", bookId);
            return null;
        }
    }

    public async Task<Dictionary<Guid, BookMetadataDto>> GetBooksMetadataAsync(IEnumerable<Guid> bookIds, CancellationToken cancellationToken = default)
    {
        var result = new Dictionary<Guid, BookMetadataDto>();
        var tasks = bookIds.Select(async id =>
        {
            var metadata = await GetBookMetadataAsync(id, cancellationToken);
            return (id, metadata);
        });

        var results = await Task.WhenAll(tasks);

        foreach (var (id, metadata) in results)
        {
            if (metadata != null)
            {
                result[id] = metadata;
            }
        }

        return result;
    }

    public async Task<bool> BookExistsAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync($"/api/books/{bookId}", cancellationToken);
            return response.IsSuccessStatusCode;
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Failed to check if book exists for BookId {BookId}", bookId);
            return false;
        }
    }

    // Internal DTO matching CatalogService response
    private class CatalogBookDto
    {
        public Guid Id { get; set; }
        public string? Title { get; set; }
        public string? Author { get; set; }
        public string? Isbn { get; set; }
        public string? Genre { get; set; }
        public bool IsAvailable { get; set; }
    }
}
