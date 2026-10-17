using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using StudentAccommodationSystem.Data; using StudentAccommodationSystem.Models;
namespace StudentAccommodationSystem.Controllers;
[Authorize] public class RentalAgreementController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller
{ public async Task<IActionResult> Index(){var q=db.RentalAgreements.Include(x=>x.Property).Include(x=>x.Room).AsQueryable();if(User.IsInRole("Student")){var u=await users.GetUserAsync(User);var s=await db.StudentProfiles.FirstAsync(x=>x.UserId==u!.Id);q=q.Where(x=>x.StudentProfileId==s.Id);}else if(User.IsInRole("Owner")){var u=await users.GetUserAsync(User);var o=await db.OwnerProfiles.FirstAsync(x=>x.UserId==u!.Id);q=q.Where(x=>x.OwnerProfileId==o.Id);}return View(await q.OrderByDescending(x=>x.StartDate).ToListAsync());}
}
