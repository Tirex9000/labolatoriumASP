using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace labapp;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.
        
        // --- Część E: Rejestracja filtra wyjątku GLOBALNIE ---
        builder.Services.AddControllersWithViews(o => o.Filters.Add<GlobalExceptionFilter>());

        // --- Część E: Rejestracja filtra akcji jako Scoped ---
        builder.Services.AddScoped<LogActionFilter>();

        // --- Część D: Rejestracja RequestStats jako Singleton ---
        builder.Services.AddSingleton<RequestStats>();

        builder.Services.AddControllersWithViews();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        
        // --- Część F: Obsługa błędów (404/500) i HSTS (tylko w produkcji) ---
        if (!app.Environment.IsDevelopment())
        {
            // Middleware do przechwycenia wyjątków i przekierowania do /Home/Error
            app.UseExceptionHandler("/Home/Error");
            // HSTS (HTTP Strict Transport Security)
            app.UseHsts();
        }

        // --- Część F: Middleware do obsługi statusów błędów (np. 404) ---
        // Przekieruje żądanie do /Home/HttpStatus?code=404 jeśli serwer zwróci np. 404
        app.UseStatusCodePagesWithReExecute("/Home/HttpStatus", "?code={0}");

        // --- Część D: Middleware licznika/czasu MUSI być na początku potoku ---
        app.UseMiddleware<RequestTimingMiddleware>();

        app.UseHttpsRedirection();
        app.UseRouting();
        app.UseAuthorization();

        app.MapStaticAssets();
        
        // Obsługa statycznych zasobów dla obszaru Admin (jeśli istnieje)
        app.MapControllerRoute("admin-area", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}");
        
        // Domyślna trasa MVC (z obsługą statycznych plików)
        app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}")
            .WithStaticAssets();

        // Trasy niestandardowe (część A/B)
        app.MapControllerRoute("books-by-year", "books/year/{year:int:min(1450)}",
            new { controller = "Book", action = "ByYear" });
            
        app.MapControllerRoute("books-by-isbn", "books/isbn/{isbn:regex(^\\d{{13}}$)}",
            new { controller = "Book", action = "ByIsbn" });
            
        app.MapControllerRoute("docs", "docs/{*path}",
            new { controller = "Home", action = "Docs" });
        
        // app.Run();
    }
}

// --- Część D: Implementacja Middleware i Stats ---

public class RequestStats
{
    private long _total;
    public long Total => Interlocked.Read(ref _total);
    public long Hit() => Interlocked.Increment(ref _total);
}

public class RequestTimingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestTimingMiddleware> _log;

    public RequestTimingMiddleware(RequestDelegate next, ILogger<RequestTimingMiddleware> log)
    { 
        _next = next; 
        _log = log; 
    }

    public async Task InvokeAsync(HttpContext ctx, RequestStats stats)
    {
        var sw = Stopwatch.StartNew();
        var n = stats.Hit();
        
        ctx.Response.OnStarting(() =>                     
        {
            ctx.Response.Headers["X-Elapsed-Ms"] = sw.ElapsedMilliseconds.ToString();
            ctx.Response.Headers["X-Request-Count"] = n.ToString();
            return Task.CompletedTask;
        });
        
        await _next(ctx);                                

        _log.LogInformation("{Method} {Path} -> {Status} w {Ms} ms", ctx.Request.Method,
            ctx.Request.Path, ctx.Response.StatusCode, sw.ElapsedMilliseconds);
    }
}

// --- Część E: Implementacja Filtrów ---

public class LogActionFilter : IActionFilter
{
    private readonly ILogger<LogActionFilter> _log;
    public LogActionFilter(ILogger<LogActionFilter> log) => _log = log;

    public void OnActionExecuting(ActionExecutingContext c) =>
        _log.LogInformation("START {Action}({Args})", c.ActionDescriptor.DisplayName,
            string.Join(", ", c.ActionArguments.Select(a => $"{a.Key}={a.Value}")));

    public void OnActionExecuted(ActionExecutedContext c) =>
        _log.LogInformation("KONIEC {Action}", c.ActionDescriptor.DisplayName);
}

public class GlobalExceptionFilter : IExceptionFilter
{
    private readonly ILogger<GlobalExceptionFilter> _log;

    public GlobalExceptionFilter(ILogger<GlobalExceptionFilter> log)
    {
        _log = log;
    }

    public void OnException(ExceptionContext context)
    {
        _log.LogError(context.Exception, "Wystąpił błąd na {Path}", context.HttpContext.Request.Path);

        // Jeśli to API (/api/...), zwróć ProblemDetails (lub prosty JSON)
        if (context.HttpContext.Request.Path.StartsWithSegments("/api"))
        {
            context.HttpContext.Response.ContentType = "application/json";
            context.Result = new ObjectResult(new { error = context.Exception.Message })
            {
                StatusCode = 500
            };
        }
        else
        {
            // Dla widoków MVC zwróć widok Error
            context.Result = new ViewResult { ViewName = "~/Views/Shared/Error.cshtml" };
            context.HttpContext.Response.StatusCode = 500;
        }
        
        context.ExceptionHandled = true;
    }
}



/*
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
        app.MapControllerRoute("admin-area", "{area:exists}/{controller=Dashboard}/{action=Index}/{id?}"); //added
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
*/