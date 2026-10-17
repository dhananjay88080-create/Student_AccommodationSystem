using Microsoft.AspNetCore.Mvc;

namespace StudentAccommodationSystem.Controllers;

public class InfoController : Controller
{
    [HttpGet]
    public IActionResult Support() => View();

    [HttpGet]
    public IActionResult Privacy() => View();

    [HttpGet]
    public IActionResult Terms() => View();

    [HttpGet]
    public IActionResult Faq() => View();
}
