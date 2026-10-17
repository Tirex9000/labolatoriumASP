namespace labapp.Models;

public class BookStoreMemory
{
    public static readonly List<Book> Books = new()
    {
        new() { Id = 1, Title = "Solaris",   Isbn = "978-83-08-06534-1", PublicationDate = new(1961, 1, 1), NumPages = 204, Price = 39.90m, Author = "Stanisław Lem" },
        new() { Id = 2, Title = "Cyberiada", Isbn = "978-83-08-06535-8", PublicationDate = new(1965, 1, 1), NumPages = 296, Price = 44.00m, Author = "Stanisław Lem" },
        new() { Id = 3, Title = "Lalka",     Isbn = "978-83-7327-000-8", PublicationDate = new(1890, 1, 1), NumPages = 720, Price = 29.50m, Author = "Bolesław Prus" },
    };
}