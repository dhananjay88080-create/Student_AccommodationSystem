using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using StudentAccommodationSystem.Data; using StudentAccommodationSystem.Models; using StudentAccommodationSystem.Services; using StudentAccommodationSystem.ViewModels;
namespace StudentAccommodationSystem.Controllers;
[Authorize] public class BookingController(ApplicationDbContext db,UserManager<ApplicationUser> users,IBookingService bookingService,INotificationService notifications):Controller
{
 [Authorize(Roles="Student")] public async Task<IActionResult> Create(int roomId){var room=await db.Rooms.Include(x=>x.Property).ThenInclude(x=>x.Owner).FirstOrDefaultAsync(x=>x.Id==roomId);return room==null?NotFound():View(new BookingVm{RoomId=roomId});}
 [HttpPost,Authorize(Roles="Student"),ValidateAntiForgeryToken] public async Task<IActionResult> Create(BookingVm m){var u=await users.GetUserAsync(User);var s=await db.StudentProfiles.FirstAsync(x=>x.UserId==u!.Id);var room=await db.Rooms.Include(x=>x.Property).ThenInclude(x=>x.Owner).FirstOrDefaultAsync(x=>x.Id==m.RoomId);if(room==null||room.Status!=RoomStatus.Available){ModelState.AddModelError("","Room is not available.");return View(m);}if(m.MoveOut<=m.MoveIn){ModelState.AddModelError("","Move-out must be after move-in.");return View(m);}var conflict=await db.Bookings.AnyAsync(x=>x.StudentProfileId==s.Id&&x.Status==BookingStatus.Approved&&m.MoveIn<x.RequestedMoveOut&&m.MoveOut>x.RequestedMoveIn);if(conflict){ModelState.AddModelError("","You already have a conflicting active booking.");return View(m);}db.Bookings.Add(new Booking{StudentProfileId=s.Id,PropertyId=room.PropertyId,RoomId=room.Id,RequestedMoveIn=m.MoveIn,RequestedMoveOut=m.MoveOut,Message=m.Message});await db.SaveChangesAsync();await notifications.NotifyAsync(room.Property.Owner.UserId,"A new booking request was submitted.","/Owner");return RedirectToAction(nameof(MyBookings));}
 [Authorize(Roles="Student")] public async Task<IActionResult> MyBookings(){var u=await users.GetUserAsync(User);var s=await db.StudentProfiles.FirstAsync(x=>x.UserId==u!.Id);return View(await db.Bookings.Include(x=>x.Property).ThenInclude(p=>p.Owner).ThenInclude(o=>o.User).Include(x=>x.Room).Include(x=>x.RentalAgreement).ThenInclude(a=>a.Payments).Where(x=>x.StudentProfileId==s.Id).OrderByDescending(x=>x.RequestDate).ToListAsync());}
 [HttpPost,Authorize(Roles="Student"),ValidateAntiForgeryToken]
 public async Task<IActionResult> DeleteRequest(int id)
 {
  var user = await users.GetUserAsync(User);
  if (user == null) return Challenge();

  var student = await db.StudentProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
  if (student == null)
  {
   TempData["Error"] = "Your student profile could not be found. Your booking request was not deleted.";
   return RedirectToAction(nameof(MyBookings));
  }

  var booking = await db.Bookings
   .Include(x => x.RentalAgreement)
   .FirstOrDefaultAsync(x => x.Id == id && x.StudentProfileId == student.Id);

  if (booking == null)
  {
   TempData["Error"] = "Booking request not found or you do not have permission to delete it.";
   return RedirectToAction(nameof(MyBookings));
  }

  // Only requests that have not been approved into a rental can be removed.
  if (booking.RentalAgreement != null || booking.Status == BookingStatus.Approved || booking.Status == BookingStatus.Completed)
  {
   TempData["Error"] = "This booking cannot be deleted because it has been approved or has a rental agreement.";
   return RedirectToAction(nameof(MyBookings));
  }

  if (booking.Status != BookingStatus.Pending && booking.Status != BookingStatus.Rejected && booking.Status != BookingStatus.Cancelled)
  {
   TempData["Error"] = "Only pending, rejected, or cancelled booking requests can be deleted.";
   return RedirectToAction(nameof(MyBookings));
  }

  db.Bookings.Remove(booking);
  await db.SaveChangesAsync();
  TempData["Success"] = "Booking request deleted successfully.";
  return RedirectToAction(nameof(MyBookings));
 }
 [Authorize(Roles="Owner")] public async Task<IActionResult> OwnerRequests(){var u=await users.GetUserAsync(User);var op=await db.OwnerProfiles.FirstAsync(x=>x.UserId==u!.Id);return View(await db.Bookings.Include(x=>x.Student).ThenInclude(x=>x.User).Include(x=>x.Property).Include(x=>x.Room).Where(x=>x.Property.OwnerProfileId==op.Id).OrderByDescending(x=>x.RequestDate).ToListAsync());}
 [HttpPost,Authorize(Roles="Owner"),ValidateAntiForgeryToken] public async Task<IActionResult> Approve(int id){var u=await users.GetUserAsync(User);var r=await bookingService.ApproveAsync(id,u!.Id);TempData[r.Ok?"Success":"Error"]=r.Message;return RedirectToAction(nameof(OwnerRequests));}
 [HttpPost,Authorize(Roles="Owner"),ValidateAntiForgeryToken] public async Task<IActionResult> Reject(int id){var u=await users.GetUserAsync(User);var b=await db.Bookings.Include(x=>x.Property).FirstOrDefaultAsync(x=>x.Id==id&&x.Property.Owner.UserId==u!.Id);if(b!=null){b.Status=BookingStatus.Rejected;await db.SaveChangesAsync();}return RedirectToAction(nameof(OwnerRequests));}
}
