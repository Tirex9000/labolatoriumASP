namespace labapp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseRouting();

        app.UseAuthorization();

        app.MapStaticAssets();
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        app.MapControllerRoute("books-by-year", "books/year/{year:int:min(1450)}",
            new { controller = "Book", action = "ByYear" });                    // ograniczenie int + min
        app.MapControllerRoute("books-by-isbn", "books/isbn/{isbn:regex(^\\d{{13}}$)}",
            new { controller = "Book", action = "ByIsbn" });                    // regex: dokładnie 13 cyfr
        app.MapControllerRoute("docs", "docs/{*path}",
            new { controller = "Home", action = "Docs" });                      // catch-all
        app.MapControllerRoute("default", "{controller=Home}/{action=Index}/{id?}");   // segmenty opcjonalne
        
        app.Run();
    }
}