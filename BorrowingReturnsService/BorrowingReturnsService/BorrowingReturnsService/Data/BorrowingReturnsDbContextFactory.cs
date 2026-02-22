using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BorrowingReturnsService.Data
{
    public class BorrowingReturnsDbContextFactory : IDesignTimeDbContextFactory<BorrowingReturnsDbContext>
    {
        public BorrowingReturnsDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<BorrowingReturnsDbContext>();
            optionsBuilder.UseNpgsql("Host=localhost;Port=5432;Database=borrowing_returns_db;Username=postgres;Password=postgres");

            return new BorrowingReturnsDbContext(optionsBuilder.Options);
        }
    }
}
