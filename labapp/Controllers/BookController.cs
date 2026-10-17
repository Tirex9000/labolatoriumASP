using labapp.Models;
using Microsoft.AspNetCore.Mvc;

namespace labapp.Controllers;

public class BookController : Controller
{
    // GET
    public IActionResult Index()
    {
        return View(BookStoreMemory.Books);
    }
}