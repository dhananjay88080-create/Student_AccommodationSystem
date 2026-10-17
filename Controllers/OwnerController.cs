using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAccommodationSystem.Data;
using StudentAccommodationSystem.Models;
using StudentAccommodationSystem.ViewModels;

namespace StudentAccommodationSystem.Controllers;

[Authorize(Roles = "Owner")]
public class OwnerController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    private async Task<OwnerProfile?> CurrentOwnerAsync()
    {
        var user = await users.GetUserAsync(User);
        if (user == null) return null;
        var owner = await db.OwnerProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
        if (owner == null) { owner = new OwnerProfile { UserId = user.Id }; db.OwnerProfiles.Add(owner); await db.SaveChangesAsync(); }
        return owner;
    }

    public async Task<IActionResult> Index()
    {
        var op = await CurrentOwnerAsync();
        if (op == null) return Challenge();
        var ownerProperties = await db.Properties.AsNoTracking().Include(p => p.Rooms).Where(x => x.OwnerProfileId == op.Id).OrderByDescending(x => x.CreatedAt).ToListAsync();
        var pids = ownerProperties.Select(x => x.Id).ToList();
        var vm = new DashboardVm { OwnerProperties = ownerProperties, TotalProperties = pids.Count, TotalRooms = await db.Rooms.CountAsync(x => pids.Contains(x.PropertyId)), AvailableRooms = await db.Rooms.CountAsync(x => pids.Contains(x.PropertyId) && x.Status == RoomStatus.Available), OccupiedRooms = await db.Rooms.CountAsync(x => pids.Contains(x.PropertyId) && x.Status == RoomStatus.Occupied), PendingBookings = await db.Bookings.CountAsync(x => pids.Contains(x.PropertyId) && x.Status == BookingStatus.Pending), ActiveTenants = await db.RentalAgreements.CountAsync(x => pids.Contains(x.PropertyId) && x.Status == AgreementStatus.Active), PendingMaintenance = await db.MaintenanceRequests.CountAsync(x => pids.Contains(x.PropertyId) && x.Status != MaintenanceStatus.Closed && x.Status != MaintenanceStatus.Resolved), MonthlyIncome = await db.Payments.Where(x => pids.Contains(x.RentalAgreement.PropertyId) && x.Status == PaymentStatus.Paid && x.Type == PaymentType.Rent).SumAsync(x => (decimal?)x.Amount) ?? 0 };
        return View(vm);
    }

    public async Task<IActionResult> Rooms()
    {
        var owner = await CurrentOwnerAsync(); if (owner == null) return Challenge();
        var rooms = await db.Rooms.Include(r => r.Property).Where(r => r.Property.OwnerProfileId == owner.Id).OrderBy(r => r.Property.Name).ThenBy(r => r.RoomNumber).ToListAsync();
        ViewBag.Properties = await db.Properties.Where(p => p.OwnerProfileId == owner.Id).OrderBy(p => p.Name).ToListAsync();
        return View(rooms);
    }

    public async Task<IActionResult> Students()
    {
        var owner = await CurrentOwnerAsync(); if (owner == null) return Challenge();
        // Fetch student IDs first, then load profiles and user details explicitly.
        // This avoids fragile Include-after-projection queries and duplicate rows when
        // a student has more than one agreement with this owner.
        var studentIds = await db.RentalAgreements
            .Where(a => a.OwnerProfileId == owner.Id)
            .Select(a => a.StudentProfileId)
            .Distinct()
            .ToListAsync();

        var students = await db.StudentProfiles
            .AsNoTracking()
            .Include(s => s.User)
            .Where(s => studentIds.Contains(s.Id))
            .OrderBy(s => s.User.FullName)
            .ToListAsync();

        ViewBag.OwnerStudentsNote = "Students with a rental agreement for one of your properties are shown here.";
        return View(students);
    }


    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var owner = await CurrentOwnerAsync();
        if (owner == null) return Challenge();

        var room = await db.Rooms.Include(r => r.Property)
            .FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return NotFound();
        if (room.Property.OwnerProfileId != owner.Id) return Forbid();

        var hasBookings = await db.Bookings.AnyAsync(b => b.RoomId == id);
        var hasAgreements = await db.RentalAgreements.AnyAsync(a => a.RoomId == id);
        var hasMaintenance = await db.MaintenanceRequests.AnyAsync(m => m.RoomId == id);
        if (hasBookings || hasAgreements || hasMaintenance)
        {
            TempData["Error"] = "This room cannot be deleted because it has booking, rental-agreement, or maintenance history.";
            return RedirectToAction(nameof(Rooms));
        }

        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        TempData["Success"] = "Room deleted successfully.";
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRoomStatus(int id, RoomStatus status)
    {
        var owner = await CurrentOwnerAsync(); if (owner == null) return Challenge();
        var room = await db.Rooms.Include(r => r.Property).FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return NotFound();
        if (room.Property.OwnerProfileId != owner.Id) return Forbid();
        room.Status = status; await db.SaveChangesAsync();
        TempData["Success"] = "Room status updated.";
        return RedirectToAction(nameof(Rooms));
    }
}
