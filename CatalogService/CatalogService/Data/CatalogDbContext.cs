using Microsoft.EntityFrameworkCore;
using CatalogService.Models;

namespace CatalogService.Data;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options)
    {
    }

    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasKey(b => b.Id);
            
            entity.Property(b => b.Title)
                .IsRequired()
                .HasMaxLength(500);
            
            entity.Property(b => b.Author)
                .IsRequired()
                .HasMaxLength(200);
            
            entity.Property(b => b.ISBN)
                .IsRequired()
                .HasMaxLength(20);
            
            entity.Property(b => b.Genre)
                .IsRequired()
                .HasMaxLength(100);
            
            entity.Property(b => b.IsAvailable)
                .IsRequired();
            
            entity.Property(b => b.CreatedAt)
                .IsRequired();

            // Create indexes for better search performance
            entity.HasIndex(b => b.ISBN);
            entity.HasIndex(b => b.Title);
            entity.HasIndex(b => b.Author);
            entity.HasIndex(b => b.Genre);
            entity.HasIndex(b => b.IsAvailable);
        });

        base.OnModelCreating(modelBuilder);
    }
}
