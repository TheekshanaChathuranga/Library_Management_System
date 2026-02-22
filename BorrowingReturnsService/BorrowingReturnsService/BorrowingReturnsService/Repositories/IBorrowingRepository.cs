using BorrowingReturnsService.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Repositories
{
    public interface IBorrowingRepository
    {
        Task<Borrowing> GetByIdAsync(Guid id);
        Task<IEnumerable<Borrowing>> GetByUserIdAsync(Guid userId);
        Task<Borrowing> AddAsync(Borrowing borrowing);
        Task<Borrowing> UpdateAsync(Borrowing borrowing);
    }
}
