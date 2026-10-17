using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAccommodationSystem.Data;
using StudentAccommodationSystem.ViewModels;
namespace StudentAccommodationSystem.Controllers;
public class HomeController(ApplicationDbContext db) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new HomeViewModel
        {
            FeaturedProperties = await db.Properties
                .Include(p => p.Rooms)
                .Include(p => p.Images)
                .Where(p => p.Status == "Approved")
                .OrderByDescending(p => p.CreatedAt)
                .Take(6)
                .ToListAsync()
        };
        return View(vm);
    }
    public IActionResult Error() => View();
}
