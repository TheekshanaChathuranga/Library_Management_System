using BorrowingReturnsService.Models;
using Microsoft.EntityFrameworkCore;

namespace BorrowingReturnsService.Data
{
    public class BorrowingReturnsDbContext : DbContext
    {
        public BorrowingReturnsDbContext(DbContextOptions<BorrowingReturnsDbContext> options) : base(options)
        {
        }

        public DbSet<Borrowing> Borrowings { get; set; }
        public DbSet<Return> Returns { get; set; }
        public DbSet<LateFee> LateFees { get; set; }
    }
}
