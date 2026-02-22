using BorrowingReturnsService.Models;
using BorrowingReturnsService.Repositories;
using System;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Services
{
    public interface ILateFeeService
    {
        Task<LateFee> CalculateLateFeeAsync(Borrowing borrowing);
        Task<decimal> GetLateFeeRateAsync();
    }

    public class LateFeeService : ILateFeeService
    {
        private readonly ILateFeeRepository _lateFeeRepository;
        private const decimal LATE_FEE_PER_DAY = 1.00m; // $1 per day

        public LateFeeService(ILateFeeRepository lateFeeRepository)
        {
            _lateFeeRepository = lateFeeRepository;
        }

        public async Task<LateFee> CalculateLateFeeAsync(Borrowing borrowing)
        {
            if (DateTime.UtcNow <= borrowing.DueDate)
            {
                return null; // No late fee if returned on time
            }

            // Check if late fee already exists
            var existingLateFee = await _lateFeeRepository.GetByBorrowingIdAsync(borrowing.Id);
            if (existingLateFee != null)
            {
                return existingLateFee;
            }

            var daysLate = (DateTime.UtcNow - borrowing.DueDate).Days;
            var feeAmount = daysLate * LATE_FEE_PER_DAY;

            var lateFee = new LateFee
            {
                BorrowingId = borrowing.Id,
                Amount = feeAmount,
                IsPaid = false
            };

            return await _lateFeeRepository.AddAsync(lateFee);
        }

        public Task<decimal> GetLateFeeRateAsync()
        {
            return Task.FromResult(LATE_FEE_PER_DAY);
        }
    }
}
