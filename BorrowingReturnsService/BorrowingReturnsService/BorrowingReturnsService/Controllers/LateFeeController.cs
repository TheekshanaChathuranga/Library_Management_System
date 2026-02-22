using BorrowingReturnsService.Dtos;
using BorrowingReturnsService.Repositories;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LateFeeController : ControllerBase
    {
        private readonly ILateFeeRepository _lateFeeRepository;
        private readonly ILogger<LateFeeController> _logger;

        public LateFeeController(ILateFeeRepository lateFeeRepository, ILogger<LateFeeController> logger)
        {
            _lateFeeRepository = lateFeeRepository;
            _logger = logger;
        }

        /// <summary>
        /// Get a late fee by ID
        /// </summary>
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var lateFee = await _lateFeeRepository.GetByIdAsync(id);
            if (lateFee == null)
            {
                return NotFound(new { message = $"Late fee with ID {id} not found" });
            }

            var lateFeeDto = new LateFeeDto
            {
                Id = lateFee.Id,
                BorrowingId = lateFee.BorrowingId,
                Amount = lateFee.Amount,
                IsPaid = lateFee.IsPaid
            };

            return Ok(lateFeeDto);
        }

        /// <summary>
        /// Get all late fees for a user
        /// </summary>
        [HttpGet("user/{userId:guid}")]
        public async Task<IActionResult> GetUserLateFees(Guid userId)
        {
            var lateFees = await _lateFeeRepository.GetByUserIdAsync(userId);

            var lateFeeDtos = lateFees.Select(lf => new LateFeeDto
            {
                Id = lf.Id,
                BorrowingId = lf.BorrowingId,
                Amount = lf.Amount,
                IsPaid = lf.IsPaid
            });

            return Ok(lateFeeDtos);
        }

        /// <summary>
        /// Get late fee by borrowing ID
        /// </summary>
        [HttpGet("borrowing/{borrowingId:guid}")]
        public async Task<IActionResult> GetByBorrowingId(Guid borrowingId)
        {
            var lateFee = await _lateFeeRepository.GetByBorrowingIdAsync(borrowingId);
            if (lateFee == null)
            {
                return NotFound(new { message = $"No late fee found for borrowing {borrowingId}" });
            }

            var lateFeeDto = new LateFeeDto
            {
                Id = lateFee.Id,
                BorrowingId = lateFee.BorrowingId,
                Amount = lateFee.Amount,
                IsPaid = lateFee.IsPaid
            };

            return Ok(lateFeeDto);
        }

        /// <summary>
        /// Mark a late fee as paid
        /// </summary>
        [HttpPatch("{id:guid}/pay")]
        public async Task<IActionResult> PayLateFee(Guid id)
        {
            var lateFee = await _lateFeeRepository.GetByIdAsync(id);
            if (lateFee == null)
            {
                return NotFound(new { message = $"Late fee with ID {id} not found" });
            }

            if (lateFee.IsPaid)
            {
                return BadRequest(new { message = "Late fee has already been paid" });
            }

            lateFee.IsPaid = true;
            await _lateFeeRepository.UpdateAsync(lateFee);

            _logger.LogInformation($"Late fee {id} marked as paid");

            var lateFeeDto = new LateFeeDto
            {
                Id = lateFee.Id,
                BorrowingId = lateFee.BorrowingId,
                Amount = lateFee.Amount,
                IsPaid = lateFee.IsPaid
            };

            return Ok(lateFeeDto);
        }
    }
}
