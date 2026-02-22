using BorrowingReturnsService.Data;
using BorrowingReturnsService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Repositories
{
    public class BorrowingRepository : IBorrowingRepository
    {
        private readonly BorrowingReturnsDbContext _context;

        public BorrowingRepository(BorrowingReturnsDbContext context)
        {
            _context = context;
        }

        public async Task<Borrowing> AddAsync(Borrowing borrowing)
        {
            _context.Borrowings.Add(borrowing);
            await _context.SaveChangesAsync();
            return borrowing;
        }

        public async Task<Borrowing> GetByIdAsync(Guid id)
        {
            return await _context.Borrowings.FindAsync(id);
        }

        public async Task<IEnumerable<Borrowing>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Borrowings.Where(b => b.UserId == userId).ToListAsync();
        }

        public async Task<Borrowing> UpdateAsync(Borrowing borrowing)
        {
            _context.Entry(borrowing).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return borrowing;
        }
    }
}
