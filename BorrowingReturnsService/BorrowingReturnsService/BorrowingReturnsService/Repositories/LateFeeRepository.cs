using BorrowingReturnsService.Data;
using BorrowingReturnsService.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BorrowingReturnsService.Repositories
{
    public class LateFeeRepository : ILateFeeRepository
    {
        private readonly BorrowingReturnsDbContext _context;

        public LateFeeRepository(BorrowingReturnsDbContext context)
        {
            _context = context;
        }

        public async Task<LateFee> AddAsync(LateFee lateFee)
        {
            _context.LateFees.Add(lateFee);
            await _context.SaveChangesAsync();
            return lateFee;
        }

        public async Task<LateFee> GetByBorrowingIdAsync(Guid borrowingId)
        {
            return await _context.LateFees.FirstOrDefaultAsync(lf => lf.BorrowingId == borrowingId);
        }

        public async Task<LateFee> GetByIdAsync(Guid id)
        {
            return await _context.LateFees.FindAsync(id);
        }

        public async Task<IEnumerable<LateFee>> GetByUserIdAsync(Guid userId)
        {
            // Get all late fees for a user's borrowings
            var borrowingIds = await _context.Borrowings
                .Where(b => b.UserId == userId)
                .Select(b => b.Id)
                .ToListAsync();

            return await _context.LateFees
                .Where(lf => borrowingIds.Contains(lf.BorrowingId))
                .ToListAsync();
        }

        public async Task<LateFee> UpdateAsync(LateFee lateFee)
        {
            _context.Entry(lateFee).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return lateFee;
        }
    }
}
