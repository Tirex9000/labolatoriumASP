using System.ComponentModel.DataAnnotations;

namespace labapp.Models;

public class BookFormViewModel
{
    [Required(ErrorMessage = "Tytuł jest wymagany")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Tytuł: 2–200 znaków")]
    [Display(Name = "Tytuł")]
    public string Title { get; set; } = "";

    [Required, Isbn(false)]                          // własny atrybut – część E
    public string Isbn { get; set; } = "";

    [DataType(DataType.Date)] [NotInFuture]   // własny atrybut – część E
    [Display(Name = "Data wydania")]
    public DateTime? PublicationDate { get; set; }

    [Range(1, 20000)] [Display(Name = "Liczba stron")]
    public int NumPages { get; set; } = 100;

    [Range(0.01, 10000, ErrorMessage = "Cena musi być dodatnia")]
    public decimal Price { get; set; }

    [Required, Display(Name = "Autor")]
    public string Author { get; set; } = "";
}