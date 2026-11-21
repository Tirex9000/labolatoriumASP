using labapp.Models;
using Microsoft.AspNetCore.Mvc;

namespace labapp.Controllers;

[ServiceFilter(typeof(LogActionFilter))]
public class BookController(IBookService books) : Controller
{
    private readonly IBookService _books = books;

    // GET: /Book?page=2&search=tolkien
    public async Task<IActionResult> Index(int page = 1, int pageSize = 10, string? search = null)
    {
        var result = await _books.GetPageAsync(page, pageSize, search);
        return View(result);                       // widok musi przyjmować PagedResult<Book>
    }

    public async Task<IActionResult> ByYear(int year)
    {
        // filtr po roku wymaga metody w serwisie, np.:
        var items = await _books.GetByYearAsync(year);
        return View("Index", items);
    }

    public async Task<IActionResult> ByIsbn(string isbn)
    {
        if (string.IsNullOrWhiteSpace(isbn)) return NotFound();

        var book = await _books.GetByIsbnAsync(isbn);
        
        if (book is null) return NotFound();

        return RedirectToAction(nameof(Details), new { id = book.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var book = await _books.GetByIdAsync(id);
        return book is null ? NotFound() : View(book);
    }

    [HttpGet]
    public IActionResult Create() => View(new BookFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        if (await _books.IsbnExistsAsync(vm.Isbn))
        {
            ModelState.AddModelError(nameof(vm.Isbn), "Ten ISBN już istnieje.");
            return View(vm);
        }

        var book = new Book
        {
            Title           = vm.Title,
            Isbn            = vm.Isbn,
            PublicationDate = vm.PublicationDate,
            NumPages        = vm.NumPages,
            Price           = vm.Price,
            Author          = vm.Author
        };

        await _books.AddAsync(book);

        TempData["Success"] = $"Dodano książkę „{vm.Title}”.";
        return RedirectToAction(nameof(Index));       // PRG
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var book = await _books.GetByIdAsync(id);
        if (book is null) return NotFound();

        return View(new BookFormViewModel
        {
            Title           = book.Title,
            Isbn            = book.Isbn,
            PublicationDate = book.PublicationDate,
            NumPages        = book.NumPages,
            Price           = book.Price,
            Author          = book.Author
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        // kluczowe dla zadania z Lab 4: wykluczamy własny rekord
        if (await _books.IsbnExistsAsync(vm.Isbn, id))
        {
            ModelState.AddModelError(nameof(vm.Isbn), "Ten ISBN już istnieje.");
            return View(vm);
        }

        var ok = await _books.UpdateAsync(new Book
        {
            Id              = id,
            Title           = vm.Title,
            Isbn            = vm.Isbn,
            PublicationDate = vm.PublicationDate,
            NumPages        = vm.NumPages,
            Price           = vm.Price,
            Author          = vm.Author
        });

        if (!ok) return NotFound();

        TempData["Success"] = $"Zaktualizowano „{vm.Title}”.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        var ok = await _books.DeleteAsync(id);
        if (!ok) return NotFound();

        TempData["Success"] = "Usunięto książkę.";
        return RedirectToAction(nameof(Index));
    }
}

// using labapp.Models;
// using Microsoft.AspNetCore.Mvc;
//
// namespace labapp.Controllers;
//
//
// [ServiceFilter(typeof(LogActionFilter))] 
// public class BookController(IBookService books) : Controller
// {
//     private readonly IBookService _books = books;
//     // wstrzykiwanie przez konstruktor
//
//     public async Task<IActionResult> ByYear(int year)
//     {
//         /* filtruj listę po roku, zwróć widok Index */
//         var booksByYear = BookStoreMemory.Books
//             .Where(b => b.PublicationDate != null && b.PublicationDate?.Year == year)
//             .ToList();
//         return View("Index", booksByYear);
//     }
//
//     public async Task<IActionResult> ByIsbn(string isbn)
//     {
//         /* znajdź książkę, przekieruj na Details lub NotFound() */
//         var book = BookStoreMemory.Books
//             .FirstOrDefault(b => b.Isbn.Replace("-", "") == isbn);
//
//         if (book == null)
//         {
//             return NotFound();
//         }
//         
//         return RedirectToAction("Details", new { id = book.Id });
//         
//     }
//     
//     // GET
//     public IActionResult Index()
//     {
//         return View(BookStoreMemory.Books);
//     }
//     
//     [HttpGet]
//     public IActionResult Create() => View(new BookFormViewModel());
//
//     [HttpPost]
//     public IActionResult Create(BookFormViewModel vm)
//     {
//         if (!ModelState.IsValid) return View(vm);                // pokaż formularz ponownie z błędami
//
//         BookStoreMemory.Books.Add(new Book
//         {
//             Id = BookStoreMemory.Books.Max(b => b.Id) + 1,
//             Title = vm.Title,
//             Isbn = vm.Isbn,
//             PublicationDate = vm.PublicationDate,
//             NumPages = vm.NumPages,
//             Price = vm.Price,
//             Author = vm.Author
//         });
//
//         TempData["Success"] = $"Dodano książkę „{vm.Title}”.";   // przetrwa jedno przekierowanie
//         return RedirectToAction(nameof(Index));                   // PRG: F5 nie wyśle formularza drugi raz
//     }
// }