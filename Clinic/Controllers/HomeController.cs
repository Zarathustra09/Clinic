using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Clinic.Models;
using Clinic.DataConnection;

namespace Clinic.Controllers;

public class HomeController : BaseController
{
    private readonly ILogger<HomeController> _logger;

    public HomeController(ILogger<HomeController> logger, SqlServerDbContext context) : base(context)
    {
        _logger = logger;
    }
    
    // Rest of your methods remain the same...

    public IActionResult Index()
    {
        return View();
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
