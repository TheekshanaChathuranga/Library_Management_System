using BorrowingReturnsService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Repositories
{
    public interface ILateFeeRepository
    {
        Task<LateFee> GetByIdAsync(Guid id);
        Task<LateFee> GetByBorrowingIdAsync(Guid borrowingId);
        Task<IEnumerable<LateFee>> GetByUserIdAsync(Guid userId);
        Task<LateFee> AddAsync(LateFee lateFee);
        Task<LateFee> UpdateAsync(LateFee lateFee);
    }
}
