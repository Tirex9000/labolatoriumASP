using labapp.Models;
using Microsoft.AspNetCore.Mvc;

namespace labapp.Controllers;

public class BookController : Controller
{
    public async Task<IActionResult> ByYear(int year)
    {
        /* filtruj listę po roku, zwróć widok Index */
        var booksByYear = BookStoreMemory.Books
            .Where(b => b.PublicationDate != null && b.PublicationDate?.Year == year)
            .ToList();
        return View("Index", booksByYear);
    }

    public async Task<IActionResult> ByIsbn(string isbn)
    {
        /* znajdź książkę, przekieruj na Details lub NotFound() */
        var book = BookStoreMemory.Books
            .FirstOrDefault(b => b.Isbn.Replace("-", "") == isbn);

        if (book == null)
        {
            return NotFound();
        }
        
        return RedirectToAction("Details", new { id = book.Id });
        
    }
    
    // GET
    public IActionResult Index()
    {
        return View(BookStoreMemory.Books);
    }
    
    [HttpGet]
    public IActionResult Create() => View(new BookFormViewModel());

    [HttpPost]
    public IActionResult Create(BookFormViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);                // pokaż formularz ponownie z błędami

        BookStoreMemory.Books.Add(new Book
        {
            Id = BookStoreMemory.Books.Max(b => b.Id) + 1,
            Title = vm.Title,
            Isbn = vm.Isbn,
            PublicationDate = vm.PublicationDate,
            NumPages = vm.NumPages,
            Price = vm.Price,
            Author = vm.Author
        });

        TempData["Success"] = $"Dodano książkę „{vm.Title}”.";   // przetrwa jedno przekierowanie
        return RedirectToAction(nameof(Index));                   // PRG: F5 nie wyśle formularza drugi raz
    }
}