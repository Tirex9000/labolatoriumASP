using labapp.Models.ef;
using Microsoft.EntityFrameworkCore;

namespace labapp.Models;

public interface IBookService
{
    Task<PagedResult<Book>> GetPageAsync(int page, int pageSize, string? search = null);
    Task<Book?> GetByIdAsync(int id);
    Task<Book> AddAsync(Book book);
    Task<bool> UpdateAsync(Book book);
    Task<bool> DeleteAsync(int id);
    Task<IReadOnlyList<Book>> GetByYearAsync(int year);
    Task<Book> GetByIsbnAsync(string isbn);
    Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null);
}

public record PagedResult<T>(IReadOnlyList<T> Items, int Page, int PageSize, int TotalCount)
{
    public int TotalPages => (int)Math.Ceiling(TotalCount / (double)PageSize);
}

public class BookService : IBookService
{
    private readonly AppDbContext _db;
    public BookService(AppDbContext db) => _db = db;

    public async Task<PagedResult<Book>> GetPageAsync(int page, int pageSize, string? search = null)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        IQueryable<Book> q = _db.Books.AsNoTracking();                 // odczyt bez śledzenia zmian
        if (!string.IsNullOrWhiteSpace(search)) q = q.Where(b => b.Title.Contains(search));
        var total = await q.CountAsync();
        var items = await q.OrderBy(b => b.Title)
            .Skip((page - 1) * pageSize).Take(pageSize)  // stronicowanie
            .ToListAsync();
        return new PagedResult<Book>(items, page, pageSize, total);
    }

    public async Task<Book?> GetByIdAsync(int id)
        => await _db.Books.FindAsync(id);

    public async Task<Book> AddAsync(Book book)
    {
        _db.Books.Add(book);
        await _db.SaveChangesAsync();
        return book;
    }

    public async Task<bool> UpdateAsync(Book book)
    {
        _db.Entry(book).State = EntityState.Modified;
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var book = await _db.Books.FindAsync(id);
        if (book is null) return false;

        _db.Books.Remove(book);
        return await _db.SaveChangesAsync() > 0;
    }

    public async Task<bool> IsbnExistsAsync(string isbn, int? excludeId = null)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return false;

        return await _db.Books.AnyAsync(b =>
            b.Isbn == isbn && (excludeId == null || b.Id != excludeId));
    }

    public async Task<Book> GetByIsbnAsync(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return null;

        var clean = isbn.Replace("-", "");

        // SQLite przechowuje tekst – porównujemy oczyszczone wartości.
        // Jeśli provider nie przetłumaczy Replace(), zadziała ToListAsync + filtr w pamięci.
        return await _db.Books.AsNoTracking()
            .Where(b => b.Isbn.Replace("-", "") == clean)
            .FirstOrDefaultAsync();
    }

    public async Task<IReadOnlyList<Book>> GetByYearAsync(int year)
    {
        // zakres dat zamiast .Year – pewne tłumaczenie na SQL także dla SQLite
        var from = new DateTime(year, 1, 1);
        var to   = from.AddYears(1);

        return await _db.Books.AsNoTracking()
            .Where(b => b.PublicationDate >= from && b.PublicationDate < to)
            .OrderBy(b => b.Title)
            .ToListAsync();
    }
    // … pozostałe metody: FindAsync, Add + SaveChangesAsync, Remove + SaveChangesAsync
}