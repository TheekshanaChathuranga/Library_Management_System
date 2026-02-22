using CatalogService.Data;
using CatalogService.Models;

namespace CatalogService.Data;

public static class DbInitializer
{
    public static async Task SeedDataAsync(CatalogDbContext context)
    {
        // Check if database is already seeded
        if (context.Books.Any())
        {
            return; // Database has been seeded
        }

        var books = new List<Book>
        {
            new Book
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Title = "The Great Gatsby",
                Author = "F. Scott Fitzgerald",
                ISBN = "978-0-7432-7356-5",
                Genre = "Classic Fiction",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Title = "To Kill a Mockingbird",
                Author = "Harper Lee",
                ISBN = "978-0-06-112008-4",
                Genre = "Classic Fiction",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("33333333-3333-3333-3333-333333333333"),
                Title = "1984",
                Author = "George Orwell",
                ISBN = "978-0-452-28423-4",
                Genre = "Dystopian Fiction",
                IsAvailable = false
            },
            new Book
            {
                Id = Guid.Parse("44444444-4444-4444-4444-444444444444"),
                Title = "Pride and Prejudice",
                Author = "Jane Austen",
                ISBN = "978-0-14-143951-8",
                Genre = "Romance",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("55555555-5555-5555-5555-555555555555"),
                Title = "The Catcher in the Rye",
                Author = "J.D. Salinger",
                ISBN = "978-0-316-76948-0",
                Genre = "Coming-of-Age Fiction",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("66666666-6666-6666-6666-666666666666"),
                Title = "The Hobbit",
                Author = "J.R.R. Tolkien",
                ISBN = "978-0-547-92822-7",
                Genre = "Fantasy",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("77777777-7777-7777-7777-777777777777"),
                Title = "Harry Potter and the Philosopher's Stone",
                Author = "J.K. Rowling",
                ISBN = "978-0-439-70818-8",
                Genre = "Fantasy",
                IsAvailable = false
            },
            new Book
            {
                Id = Guid.Parse("88888888-8888-8888-8888-888888888888"),
                Title = "The Da Vinci Code",
                Author = "Dan Brown",
                ISBN = "978-0-307-47927-1",
                Genre = "Mystery Thriller",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("99999999-9999-9999-9999-999999999999"),
                Title = "The Alchemist",
                Author = "Paulo Coelho",
                ISBN = "978-0-06-231500-7",
                Genre = "Fiction",
                IsAvailable = true
            },
            new Book
            {
                Id = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Title = "Brave New World",
                Author = "Aldous Huxley",
                ISBN = "978-0-06-085052-4",
                Genre = "Dystopian Fiction",
                IsAvailable = true
            }
        };

        context.Books.AddRange(books);
        await context.SaveChangesAsync();
    }
}
