using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StudentAccommodationSystem.Data;
using StudentAccommodationSystem.Models;
using StudentAccommodationSystem.ViewModels;

namespace StudentAccommodationSystem.Controllers;

[Authorize(Roles = "Administrator")]
public class AdminController(ApplicationDbContext db, UserManager<ApplicationUser> users) : Controller
{
    public async Task<IActionResult> Index()
    {
        var vm = new DashboardVm
        {
            TotalStudents = (await users.GetUsersInRoleAsync("Student")).Count,
            TotalOwners = (await users.GetUsersInRoleAsync("Owner")).Count,
            TotalProperties = await db.Properties.CountAsync(),
            TotalRooms = await db.Rooms.CountAsync(),
            AvailableRooms = await db.Rooms.CountAsync(x => x.Status == RoomStatus.Available),
            OccupiedRooms = await db.Rooms.CountAsync(x => x.Status == RoomStatus.Occupied),
            PendingApprovals = await db.Properties.CountAsync(x => x.Status == "Pending"),
            ActiveAgreements = await db.RentalAgreements.CountAsync(x => x.Status == AgreementStatus.Active),
            TotalPayments = await db.Payments.Where(x => x.Status == PaymentStatus.Paid).SumAsync(x => (decimal?)x.Amount) ?? 0,
            PendingMaintenance = await db.MaintenanceRequests.CountAsync(x => x.Status != MaintenanceStatus.Closed && x.Status != MaintenanceStatus.Resolved)
        };
        return View(vm);
    }

    public async Task<IActionResult> Students() => View(await users.GetUsersInRoleAsync("Student"));
    public async Task<IActionResult> Owners() => View(await users.GetUsersInRoleAsync("Owner"));
    public async Task<IActionResult> Rooms() => View(await db.Rooms.Include(r => r.Property).ThenInclude(p => p.Owner).ThenInclude(o => o.User).OrderBy(r => r.Property.Name).ThenBy(r => r.RoomNumber).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetUserActive(string id, bool isActive)
    {
        if (string.IsNullOrWhiteSpace(id)) return BadRequest("Missing account ID.");

        // Confirm the account exists and only allow Student/Owner accounts to be changed.
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null) return NotFound("Account was not found in the configured database.");

        var isStudent = await users.IsInRoleAsync(user, "Student");
        var isOwner = await users.IsInRoleAsync(user, "Owner");
        if (!isStudent && !isOwner) return BadRequest("Only Student and Owner accounts can be changed here.");

        // Write the status directly through the EF database context. This avoids
        // a stale Identity user-store result and makes IsActive the source of truth
        // used by AccountController.Login.
        var newLockoutEnd = isActive ? (DateTimeOffset?)null : DateTimeOffset.MaxValue;
        var affected = await db.Users
            .Where(u => u.Id == id)
            .ExecuteUpdateAsync(setters => setters
                .SetProperty(u => u.IsActive, isActive)
                .SetProperty(u => u.LockoutEnabled, true)
                .SetProperty(u => u.LockoutEnd, newLockoutEnd)
                .SetProperty(u => u.AccessFailedCount, isActive ? 0 : user.AccessFailedCount));

        var savedUser = await db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
        var redirectAction = isStudent ? nameof(Students) : nameof(Owners);
        if (affected != 1 || savedUser == null || savedUser.IsActive != isActive ||
            (isActive && savedUser.LockoutEnd.HasValue && savedUser.LockoutEnd.Value > DateTimeOffset.UtcNow))
        {
            TempData["Error"] = $"Could not verify the status change for {user.Email}. No success was reported. Confirm the database connection in appsettings.json and refresh the page.";
        }
        else
        {
            TempData["Success"] = $"{(isActive ? "Enabled" : "Disabled")} account {savedUser.Email}. Saved status: {(savedUser.IsActive ? "Active" : "Disabled")}.";
        }

        return RedirectToAction(redirectAction);
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteRoom(int id)
    {
        var room = await db.Rooms.FirstOrDefaultAsync(r => r.Id == id);
        if (room == null) return NotFound();

        var hasBookings = await db.Bookings.AnyAsync(b => b.RoomId == id);
        var hasAgreements = await db.RentalAgreements.AnyAsync(a => a.RoomId == id);
        var hasMaintenance = await db.MaintenanceRequests.AnyAsync(m => m.RoomId == id);
        if (hasBookings || hasAgreements || hasMaintenance)
        {
            TempData["Error"] = "This room cannot be deleted because it has booking, rental-agreement, or maintenance history. Keep the records for audit purposes.";
            return RedirectToAction(nameof(Rooms));
        }

        db.Rooms.Remove(room);
        await db.SaveChangesAsync();
        TempData["Success"] = "Room deleted successfully.";
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteAllRooms()
    {
        // Preserve rooms referenced by bookings, agreements, or maintenance records.
        var rooms = await db.Rooms
            .Where(r => !db.Bookings.Any(b => b.RoomId == r.Id)
                     && !db.RentalAgreements.Any(a => a.RoomId == r.Id)
                     && !db.MaintenanceRequests.Any(m => m.RoomId == r.Id))
            .ToListAsync();

        var deletedCount = rooms.Count;
        db.Rooms.RemoveRange(rooms);
        await db.SaveChangesAsync();

        var remainingCount = await db.Rooms.CountAsync();
        TempData["Success"] = $"Deleted {deletedCount} room(s) without linked records.";
        if (remainingCount > 0)
            TempData["Error"] = $"{remainingCount} room(s) remain because they have booking, rental-agreement, or maintenance history.";
        return RedirectToAction(nameof(Rooms));
    }

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> SetRoomStatus(int id, RoomStatus status)
    {
        var room = await db.Rooms.FindAsync(id);
        if (room == null) return NotFound();
        room.Status = status;
        await db.SaveChangesAsync();
        TempData["Success"] = "Room status updated.";
        return RedirectToAction(nameof(Rooms));
    }

    public async Task<IActionResult> Properties() => View(await db.Properties.Include(x => x.Owner).ThenInclude(x => x.User).ToListAsync());

    [HttpPost, ValidateAntiForgeryToken]
    public async Task<IActionResult> ApproveProperty(int id)
    {
        var p = await db.Properties.FindAsync(id);
        if (p != null) { p.Status = "Approved"; await db.SaveChangesAsync(); }
        return RedirectToAction(nameof(Properties));
    }
}
