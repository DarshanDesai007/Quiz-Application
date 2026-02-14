using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Quiz_Application.Models;

namespace Quiz_Application.Controllers;

public class HomeController : Controller
{
    public IActionResult Index() => View();

    public IActionResult Quiz() => View();

    public IActionResult Grid() => View();

    public IActionResult Summary() => View();

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
        => View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
}
