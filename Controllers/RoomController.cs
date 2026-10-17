using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using StudentAccommodationSystem.Data; using StudentAccommodationSystem.Models;
namespace StudentAccommodationSystem.Controllers;
[Authorize(Roles="Owner,Administrator")] public class RoomController(ApplicationDbContext db,UserManager<ApplicationUser> users):Controller
{ public async Task<IActionResult> Index(int propertyId){var q=db.Rooms.Where(x=>x.PropertyId==propertyId).AsQueryable();if(User.IsInRole("Owner")){var u=await users.GetUserAsync(User);var o=await db.OwnerProfiles.FirstAsync(x=>x.UserId==u!.Id);q=q.Where(x=>x.Property.OwnerProfileId==o.Id);}ViewBag.PropertyId=propertyId;return View(await q.ToListAsync());}
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> Create(Room r)
 {
     var ownerMode = User.IsInRole("Owner");
     if (ownerMode)
     {
         var u = await users.GetUserAsync(User);
         if (u == null) return Challenge();
         var o = await db.OwnerProfiles.FirstOrDefaultAsync(x => x.UserId == u.Id);
         if (o == null) { TempData["Error"] = "Your account does not have an owner profile. Please contact the administrator."; return RedirectToAction("Rooms", "Owner"); }
         if (!await db.Properties.AnyAsync(x => x.Id == r.PropertyId && x.OwnerProfileId == o.Id)) return Forbid();
     }
     if (!ModelState.IsValid)
     {
         var validationErrors = string.Join(" ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).Where(e => !string.IsNullOrWhiteSpace(e)));
         TempData["Error"] = "Could not add the room. " + (string.IsNullOrWhiteSpace(validationErrors) ? "Check room number, room type, capacity, and monthly rent." : validationErrors);
         return ownerMode ? RedirectToAction("Rooms", "Owner") : RedirectToAction(nameof(Index), new { propertyId = r.PropertyId });
     }
     db.Rooms.Add(r);
     await db.SaveChangesAsync();
     TempData["Success"] = "Room added successfully.";
     return ownerMode ? RedirectToAction("Rooms", "Owner") : RedirectToAction(nameof(Index), new { propertyId = r.PropertyId });
 }
 [HttpPost,ValidateAntiForgeryToken] public async Task<IActionResult> SetStatus(int id,RoomStatus status){var r=await db.Rooms.Include(x=>x.Property).FirstOrDefaultAsync(x=>x.Id==id);if(r==null)return NotFound();if(User.IsInRole("Owner")){var u=await users.GetUserAsync(User);var o=await db.OwnerProfiles.FirstAsync(x=>x.UserId==u!.Id);if(r.Property.OwnerProfileId!=o.Id)return Forbid();}r.Status=status;await db.SaveChangesAsync();return RedirectToAction(nameof(Index),new{propertyId=r.PropertyId});}
}
