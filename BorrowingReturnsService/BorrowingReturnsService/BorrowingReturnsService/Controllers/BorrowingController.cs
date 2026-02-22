using BorrowingReturnsService.Dtos;
using BorrowingReturnsService.Models;
using BorrowingReturnsService.Repositories;
using BorrowingReturnsService.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Controllers
{
    [ApiController]
    [Route("api/borrowing")]
    public class BorrowingController : ControllerBase
    {
        private readonly IBorrowingRepository _borrowingRepository;
        private readonly ILateFeeRepository _lateFeeRepository;
        private readonly ILateFeeService _lateFeeService;
        private readonly ICatalogClient _catalogClient;
        private readonly IInventoryClient _inventoryClient;
        private readonly IUserIdentityClient _userIdentityClient;
        private readonly ILogger<BorrowingController> _logger;

        public BorrowingController(
            IBorrowingRepository borrowingRepository,
            ILateFeeRepository lateFeeRepository,
            ILateFeeService lateFeeService,
            ICatalogClient catalogClient,
            IInventoryClient inventoryClient,
            IUserIdentityClient userIdentityClient,
            ILogger<BorrowingController> logger)
        {
            _borrowingRepository = borrowingRepository;
            _lateFeeRepository = lateFeeRepository;
            _lateFeeService = lateFeeService;
            _catalogClient = catalogClient;
            _inventoryClient = inventoryClient;
            _userIdentityClient = userIdentityClient;
            _logger = logger;
        }

        /// <summary>
        /// Borrow a book (physical or digital)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(BorrowingDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> BorrowBook([FromBody] CreateBorrowingDto createBorrowingDto)
        {
            try
            {
                // Step 1: Validate user exists and is active
                var user = await _userIdentityClient.GetUserAsync(createBorrowingDto.UserId);
                if (user == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "User not found",
                        Detail = $"User with ID {createBorrowingDto.UserId} not found.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                if (!user.IsActive)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "User inactive",
                        Detail = $"User account is inactive. Please contact library administration.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                _logger.LogInformation($"User validation passed for {user.Email} (ID: {createBorrowingDto.UserId})");

                // Step 1.5: Check borrowing limit (Max 5 active loans)
                var userBorrowings = await _borrowingRepository.GetByUserIdAsync(createBorrowingDto.UserId);
                var activeBorrowingsCount = userBorrowings.Count(b => !b.IsReturned);
                
                if (activeBorrowingsCount >= 5)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Borrowing limit reached",
                        Detail = "You have reached the maximum limit of 5 active loans. Please return some books before borrowing more.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Step 2: Check for unpaid late fees
                var userLateFees = await _lateFeeRepository.GetByUserIdAsync(createBorrowingDto.UserId);
                var unpaidFees = userLateFees.Where(lf => !lf.IsPaid).ToList();
                if (unpaidFees.Any())
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Unpaid late fees",
                        Detail = $"User has {unpaidFees.Count} unpaid late fee(s) totaling ${unpaidFees.Sum(f => f.Amount):F2}. Please pay outstanding fees before borrowing.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Step 3: Get book metadata from CatalogService
                var book = await _catalogClient.GetBookByIdAsync(createBorrowingDto.BookId);
                if (book == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Book not found",
                        Detail = $"Book with ID {createBorrowingDto.BookId} not found in catalog.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                // Step 4: Check inventory availability for requested channel
                var inventory = await _inventoryClient.GetInventoryAsync(createBorrowingDto.BookId);
                if (inventory == null)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Inventory not found",
                        Detail = $"No inventory record found for book {createBorrowingDto.BookId}.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                var availableCount = createBorrowingDto.Channel == BorrowChannel.Physical 
                    ? inventory.PhysicalAvailable 
                    : inventory.DigitalAvailable;

                if (availableCount <= 0)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Book unavailable",
                        Detail = $"{createBorrowingDto.Channel} copy of '{book.Title}' is not available for borrowing.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Step 5: Create borrowing record
                var borrowing = new Borrowing
                {
                    UserId = createBorrowingDto.UserId,
                    BookId = createBorrowingDto.BookId,
                    Channel = createBorrowingDto.Channel,
                    BorrowedAt = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow.AddDays(14), // 2 weeks borrowing period
                    IsReturned = false
                };

                var newBorrowing = await _borrowingRepository.AddAsync(borrowing);
                _logger.LogInformation($"Created borrowing record {newBorrowing.Id} for user {createBorrowingDto.UserId}, book {createBorrowingDto.BookId}, channel {createBorrowingDto.Channel}");

                // Step 6: Adjust inventory
                try
                {
                    var updatedInventory = await _inventoryClient.BorrowAsync(
                        createBorrowingDto.BookId, 
                        createBorrowingDto.Channel, 
                        newBorrowing.Id.ToString());
                    
                    _logger.LogInformation($"Inventory adjusted for book {createBorrowingDto.BookId}, channel {createBorrowingDto.Channel}");

                    // Step 7: Update catalog availability
                    // If BOTH physical and digital are 0, mark as unavailable
                    if (updatedInventory.PhysicalAvailable <= 0 && updatedInventory.DigitalAvailable <= 0)
                    {
                        await _catalogClient.UpdateBookAvailabilityAsync(createBorrowingDto.BookId, false);
                        _logger.LogInformation($"Updated catalog availability for book {createBorrowingDto.BookId} to unavailable");
                    }

                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to adjust inventory for book {createBorrowingDto.BookId}");
                    // Consider compensating transaction: delete borrowing record
                    return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                    {
                        Title = "Inventory update failed",
                        Detail = "Failed to update inventory. Please try again.",
                        Status = StatusCodes.Status500InternalServerError
                    });
                }

                var borrowingDto = new BorrowingDto
                {
                    Id = newBorrowing.Id,
                    UserId = newBorrowing.UserId,
                    BookId = newBorrowing.BookId,
                    Channel = newBorrowing.Channel,
                    BorrowedAt = newBorrowing.BorrowedAt,
                    DueDate = newBorrowing.DueDate,
                    IsReturned = newBorrowing.IsReturned
                };

                return CreatedAtAction(nameof(GetBorrowingById), new { id = borrowingDto.Id }, borrowingDto);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Inventory update failed: {Message}", ex.Message);
                return BadRequest(new ProblemDetails
                {
                    Title = "Inventory update failed",
                    Detail = ex.Message,
                    Status = StatusCodes.Status400BadRequest
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing borrow request");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = $"Error: {ex.Message} | Stack: {ex.StackTrace} | Inner: {ex.InnerException?.Message}",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Get a borrowing by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(BorrowingDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBorrowingById(Guid id)
        {
            var borrowing = await _borrowingRepository.GetByIdAsync(id);
            if (borrowing == null)
            {
                return NotFound(new { message = $"Borrowing with ID {id} not found" });
            }

            var borrowingDto = new BorrowingDto
            {
                Id = borrowing.Id,
                UserId = borrowing.UserId,
                BookId = borrowing.BookId,
                Channel = borrowing.Channel,
                BorrowedAt = borrowing.BorrowedAt,
                DueDate = borrowing.DueDate,
                IsReturned = borrowing.IsReturned
            };

            return Ok(borrowingDto);
        }

        /// <summary>
        /// Return a borrowed book
        /// </summary>
        [HttpPost("{id:guid}/return")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ReturnBook(Guid id)
        {
            try
            {
                // Step 1: Get borrowing record
                var borrowing = await _borrowingRepository.GetByIdAsync(id);
                if (borrowing == null)
                {
                    return NotFound(new ProblemDetails
                    {
                        Title = "Borrowing not found",
                        Detail = $"Borrowing record with ID {id} not found.",
                        Status = StatusCodes.Status404NotFound
                    });
                }

                if (borrowing.IsReturned)
                {
                    return BadRequest(new ProblemDetails
                    {
                        Title = "Already returned",
                        Detail = "This book has already been returned.",
                        Status = StatusCodes.Status400BadRequest
                    });
                }

                // Step 2: Mark as returned
                borrowing.IsReturned = true;
                await _borrowingRepository.UpdateAsync(borrowing);
                _logger.LogInformation($"Marked borrowing {id} as returned");

                // Step 3: Adjust inventory
                try
                {
                    var updatedInventory = await _inventoryClient.ReturnAsync(
                        borrowing.BookId, 
                        borrowing.Channel, 
                        borrowing.Id.ToString());
                    
                    _logger.LogInformation($"Inventory adjusted for return of book {borrowing.BookId}, channel {borrowing.Channel}");

                    // Step 4: Update catalog availability
                    // If ANY copy is available, mark as available
                    if (updatedInventory.PhysicalAvailable > 0 || updatedInventory.DigitalAvailable > 0)
                    {
                        await _catalogClient.UpdateBookAvailabilityAsync(borrowing.BookId, true);
                        _logger.LogInformation($"Updated catalog availability for book {borrowing.BookId} to available");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Failed to adjust inventory for book return {borrowing.BookId}");
                    // Non-fatal: continue with return process
                }

                // Step 5: Calculate late fees if overdue
                LateFee lateFee = null;
                if (DateTime.UtcNow > borrowing.DueDate)
                {
                    lateFee = await _lateFeeService.CalculateLateFeeAsync(borrowing);
                    if (lateFee != null)
                    {
                        _logger.LogInformation($"Late fee of ${lateFee.Amount:F2} created for borrowing {id}");
                    }
                }

                var response = new
                {
                    borrowingId = id,
                    bookId = borrowing.BookId,
                    channel = borrowing.Channel,
                    returnedAt = DateTime.UtcNow,
                    borrowedAt = borrowing.BorrowedAt,
                    dueDate = borrowing.DueDate,
                    daysLate = lateFee != null ? (DateTime.UtcNow - borrowing.DueDate).Days : 0,
                    lateFee = lateFee != null ? new
                    {
                        id = lateFee.Id,
                        amount = lateFee.Amount,
                        isPaid = lateFee.IsPaid
                    } : null
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing return request");
                return StatusCode(StatusCodes.Status500InternalServerError, new ProblemDetails
                {
                    Title = "Internal server error",
                    Detail = "An error occurred while processing your request.",
                    Status = StatusCodes.Status500InternalServerError
                });
            }
        }

        /// <summary>
        /// Get all borrowings for a user
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<BorrowingDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserBorrowings(Guid userId)
        {
            var borrowings = await _borrowingRepository.GetByUserIdAsync(userId);

            var borrowingDtos = borrowings.Select(b => new BorrowingDto
            {
                Id = b.Id,
                UserId = b.UserId,
                BookId = b.BookId,
                Channel = b.Channel,
                BorrowedAt = b.BorrowedAt,
                DueDate = b.DueDate,
                IsReturned = b.IsReturned
            });

            return Ok(borrowingDtos);
        }

        /// <summary>
        /// Get total unpaid late fees for a user
        /// </summary>
        [HttpGet("fees/user/{userId:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserLateFees(Guid userId)
        {
            var userLateFees = await _lateFeeRepository.GetByUserIdAsync(userId);
            var unpaidFees = userLateFees.Where(lf => !lf.IsPaid).ToList();
            
            return Ok(new 
            { 
                UserId = userId,
                TotalUnpaidAmount = unpaidFees.Sum(f => f.Amount),
                UnpaidCount = unpaidFees.Count
            });
        }
    }
}
