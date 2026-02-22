using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public class InventoryDbContext : DbContext
{
    public InventoryDbContext(DbContextOptions<InventoryDbContext> options) : base(options)
    {
    }

    public DbSet<BookInventory> Inventories => Set<BookInventory>();
    public DbSet<InventoryMovement> Movements => Set<InventoryMovement>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<BookInventory>(entity =>
        {
            entity.HasIndex(x => x.BookId).IsUnique();
            entity.Property(x => x.CreatedUtc).HasDefaultValueSql("CURRENT_TIMESTAMP");
            // LastUpdatedUtc is managed by application code, not database
            entity.HasMany(x => x.Movements)
                  .WithOne(x => x.Inventory!)
                  .HasForeignKey(x => x.BookInventoryId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<InventoryMovement>(entity =>
        {
            entity.Property(x => x.Channel).HasConversion<int>();
            entity.Property(x => x.Direction).HasConversion<int>();
            entity.Property(x => x.Reference).HasMaxLength(256);
            // TimestampUtc is managed by application code, not database
        });
    }
}
