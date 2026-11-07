using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using labapp.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace labapp.Controllers;

public class RouteInfo
{
    public string Pattern { get; set; } = "";
    public string Controller { get; set; } = "";
    public string Action { get; set; } = "";
    public string? Area { get; set; }
    public string Methods { get; set; } = "";
}

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Routes([FromServices] IActionDescriptorCollectionProvider provider)
    {
        var routes = provider.ActionDescriptors.Items
            .OfType<ControllerActionDescriptor>()
            .Select(r => new RouteInfo
            {
                Pattern = "/" + (r.AttributeRouteInfo?.Template ?? "(konwencjonalna)"),
                Action = r.ActionName,
                Controller = r.ControllerName,
                Methods = string.Join(", ",
                    r.ActionConstraints?
                        .OfType<HttpMethodActionConstraint>()
                        .SelectMany(c => c.HttpMethods ?? new[] { "*" })
                    ?? new[] { "*" }),
                Area = r.RouteValues.TryGetValue("area", out var area) ? area : null
            })
            .ToList();

        return View(routes);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
    
    public IActionResult HttpStatus(int code)
    {
        Response.StatusCode = code;
        // Możesz zwrócić widok z opisem błędu
        return View($"Error{code}"); 
    }

    public IActionResult Boom() => throw new InvalidOperationException("Celowy wyjątek");
}


/*
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using labapp.Models;
using Microsoft.AspNetCore.Mvc.ActionConstraints;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace labapp.Controllers;

public class RouteInfo
{
    public string Pattern { get; set; }
    public string Controller  { get; set; }
    public string Action  { get; set; }
    public string? Area  { get; set; }
    public string Methods { get; set; }
}

public class HomeController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult Routes([FromServices] IActionDescriptorCollectionProvider provider)
    {
        var routes = provider.ActionDescriptors.Items.OfType<ControllerActionDescriptor>()
            .Select(r =>
            {
                return new RouteInfo()
                {
                    Pattern: "/" + (r.AttributeRouteInfo?.Template ?? "Konwencjonalna"),
                    Action: r.ActionName,
                    Controller: r.ControllerName,
                    HttpMethods: string.Join(", ", r.ActionConstraints?.OfType<HttpMethodActionConstraint>()).SelectMany(c => c.HttpMethods ?? ["*"]) ?? Array.Empty<string>(),
                    Area = r.RouteValuse.TrySetValue("area", out var area) ? area : null
                };
            });
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
*/