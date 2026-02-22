using InventoryService.Models;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Data;

public static class DbInitializer
{
    public static async Task SeedAsync(InventoryDbContext context)
    {
        await context.Database.MigrateAsync();

        // Check for each book and add if missing
        var existingBookIds = await context.Inventories.Select(i => i.BookId).ToListAsync();

        // Note: BookIds should match existing books in CatalogService
        // If these books don't exist in CatalogService, inventory creation will fail validation
        var samples = new List<BookInventory>
        {
            new()
            {
                BookId = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                PhysicalTotal = 5,
                PhysicalAvailable = 5,
                DigitalTotal = 25,
                DigitalAvailable = 25
            },
            new()
            {
                BookId = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                PhysicalTotal = 3,
                PhysicalAvailable = 3,
                DigitalTotal = 15,
                DigitalAvailable = 15
            },
            new()
            {
                BookId = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                PhysicalTotal = 2,
                PhysicalAvailable = 0, // Matches Catalog IsAvailable=false
                DigitalTotal = 10,
                DigitalAvailable = 10
            },
            new()
            {
                BookId = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                PhysicalTotal = 4,
                PhysicalAvailable = 4,
                DigitalTotal = 20,
                DigitalAvailable = 20
            },
            new()
            {
                BookId = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                PhysicalTotal = 6,
                PhysicalAvailable = 6,
                DigitalTotal = 12,
                DigitalAvailable = 12
            },
            new()
            {
                BookId = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                PhysicalTotal = 8,
                PhysicalAvailable = 8,
                DigitalTotal = 30,
                DigitalAvailable = 30
            },
            new()
            {
                BookId = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                PhysicalTotal = 10,
                PhysicalAvailable = 0, // Matches Catalog IsAvailable=false
                DigitalTotal = 50,
                DigitalAvailable = 50
            },
            new()
            {
                BookId = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                PhysicalTotal = 7,
                PhysicalAvailable = 7,
                DigitalTotal = 14,
                DigitalAvailable = 14
            },
            new()
            {
                BookId = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                PhysicalTotal = 5,
                PhysicalAvailable = 5,
                DigitalTotal = 25,
                DigitalAvailable = 25
            },
            new()
            {
                BookId = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                PhysicalTotal = 3,
                PhysicalAvailable = 3,
                DigitalTotal = 15,
                DigitalAvailable = 15
            }
        };

        var newInventory = samples.Where(s => !existingBookIds.Contains(s.BookId)).ToList();
        
        if (newInventory.Any())
        {
            await context.Inventories.AddRangeAsync(newInventory);
            await context.SaveChangesAsync();
        }
    }
}
