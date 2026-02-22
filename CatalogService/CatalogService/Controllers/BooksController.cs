using Microsoft.AspNetCore.Mvc;
using CatalogService.Dtos;
using CatalogService.Models;
using CatalogService.Repositories;

namespace CatalogService.Controllers;

[ApiController]
[Route("api/[controller]")]
// TODO: Add [Authorize] attribute for mTLS authentication later
public class BooksController : ControllerBase
{
    private readonly IBookRepository _repository;
    private readonly ILogger<BooksController> _logger;

    public BooksController(IBookRepository repository, ILogger<BooksController> logger)
    {
        _repository = repository;
        _logger = logger;
    }

    /// <summary>
    /// Search and filter books with pagination
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(SearchResultDto<BookDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Search(
        [FromQuery] string? q,
        [FromQuery] string? author,
        [FromQuery] string? genre,
        [FromQuery] string? isbn,
        [FromQuery] bool? available,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? sortBy = "title",
        [FromQuery] bool desc = false)
    {
        // Validate pagination parameters
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 20;
        if (pageSize > 100) pageSize = 100; // Max page size

        var (items, total) = await _repository.SearchAsync(q, author, genre, isbn, available, page, pageSize, sortBy, desc);

        var bookDtos = items.Select(b => new BookDto
        {
            Id = b.Id,
            Title = b.Title,
            Author = b.Author,
            ISBN = b.ISBN,
            Genre = b.Genre,
            IsAvailable = b.IsAvailable,
            CreatedAt = b.CreatedAt,
            UpdatedAt = b.UpdatedAt
        });

        var result = new SearchResultDto<BookDto>
        {
            Total = total,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(total / (double)pageSize),
            Items = bookDtos
        };

        return Ok(result);
    }

    /// <summary>
    /// Get a book by ID
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        _logger.LogInformation($"GetById called with ID: {id}");
        var book = await _repository.GetByIdAsync(id);
        _logger.LogInformation($"Book found: {book != null}");
        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} not found" });
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Genre = book.Genre,
            IsAvailable = book.IsAvailable,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };

        return Ok(bookDto);
    }

    /// <summary>
    /// Get a book by ISBN
    /// </summary>
    [HttpGet("isbn/{isbn}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByISBN(string isbn)
    {
        var book = await _repository.GetByISBNAsync(isbn);
        if (book == null)
        {
            return NotFound(new { message = $"Book with ISBN {isbn} not found" });
        }

        var bookDto = new BookDto
        {
            Id = book.Id,
            Title = book.Title,
            Author = book.Author,
            ISBN = book.ISBN,
            Genre = book.Genre,
            IsAvailable = book.IsAvailable,
            CreatedAt = book.CreatedAt,
            UpdatedAt = book.UpdatedAt
        };

        return Ok(bookDto);
    }

    /// <summary>
    /// Create a new book
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateBookDto createBookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if ISBN already exists
        var existingBook = await _repository.GetByISBNAsync(createBookDto.ISBN);
        if (existingBook != null)
        {
            return BadRequest(new { message = $"A book with ISBN {createBookDto.ISBN} already exists" });
        }

        var book = new Book
        {
            Title = createBookDto.Title,
            Author = createBookDto.Author,
            ISBN = createBookDto.ISBN,
            Genre = createBookDto.Genre,
            IsAvailable = createBookDto.IsAvailable
        };

        var createdBook = await _repository.AddAsync(book);

        var bookDto = new BookDto
        {
            Id = createdBook.Id,
            Title = createdBook.Title,
            Author = createdBook.Author,
            ISBN = createdBook.ISBN,
            Genre = createdBook.Genre,
            IsAvailable = createdBook.IsAvailable,
            CreatedAt = createdBook.CreatedAt,
            UpdatedAt = createdBook.UpdatedAt
        };

        return CreatedAtAction(nameof(GetById), new { id = bookDto.Id }, bookDto);
    }

    /// <summary>
    /// Update an existing book
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateBookDto updateBookDto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var book = new Book
        {
            Id = id,
            Title = updateBookDto.Title,
            Author = updateBookDto.Author,
            ISBN = updateBookDto.ISBN,
            Genre = updateBookDto.Genre,
            IsAvailable = updateBookDto.IsAvailable
        };

        var updatedBook = await _repository.UpdateAsync(book);
        if (updatedBook == null)
        {
            return NotFound(new { message = $"Book with ID {id} not found" });
        }

        var bookDto = new BookDto
        {
            Id = updatedBook.Id,
            Title = updatedBook.Title,
            Author = updatedBook.Author,
            ISBN = updatedBook.ISBN,
            Genre = updatedBook.Genre,
            IsAvailable = updatedBook.IsAvailable,
            CreatedAt = updatedBook.CreatedAt,
            UpdatedAt = updatedBook.UpdatedAt
        };

        return Ok(bookDto);
    }

    /// <summary>
    /// Delete a book
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _repository.DeleteAsync(id);
        if (!success)
        {
            return NotFound(new { message = $"Book with ID {id} not found" });
        }

        return NoContent();
    }

    /// <summary>
    /// Update book availability status
    /// </summary>
    [HttpPatch("{id:guid}/availability")]
    [ProducesResponseType(typeof(BookDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateAvailability(Guid id, [FromBody] bool isAvailable)
    {
        var book = await _repository.GetByIdAsync(id);
        if (book == null)
        {
            return NotFound(new { message = $"Book with ID {id} not found" });
        }

        book.IsAvailable = isAvailable;
        var updatedBook = await _repository.UpdateAsync(book);

        var bookDto = new BookDto
        {
            Id = updatedBook!.Id,
            Title = updatedBook.Title,
            Author = updatedBook.Author,
            ISBN = updatedBook.ISBN,
            Genre = updatedBook.Genre,
            IsAvailable = updatedBook.IsAvailable,
            CreatedAt = updatedBook.CreatedAt,
            UpdatedAt = updatedBook.UpdatedAt
        };

        return Ok(bookDto);
    }
}
