using Microsoft.AspNetCore.Authorization; using Microsoft.AspNetCore.Identity; using Microsoft.AspNetCore.Mvc; using Microsoft.EntityFrameworkCore; using StudentAccommodationSystem.Data; using StudentAccommodationSystem.Models; using StudentAccommodationSystem.Services; using StudentAccommodationSystem.ViewModels;
namespace StudentAccommodationSystem.Controllers;
[Authorize] public class PaymentController(ApplicationDbContext db,UserManager<ApplicationUser> users,IPaymentService payments):Controller
{
 [Authorize(Roles="Student,Owner,Administrator")]
 public async Task<IActionResult> Index()
 {
  if (User.IsInRole("Student"))
  {
   var user = await users.GetUserAsync(User);
   if (user == null) return Challenge();
   var student = await db.StudentProfiles.FirstOrDefaultAsync(x => x.UserId == user.Id);
   if (student == null) return View("StudentIndex", new StudentPaymentsVm());

   var agreements = await db.RentalAgreements
    .AsNoTracking()
    .Include(a => a.Property).ThenInclude(p => p.Owner).ThenInclude(o => o.User)
    .Include(a => a.Room)
    .Include(a => a.Payments)
    .Where(a => a.StudentProfileId == student.Id)
    .OrderByDescending(a => a.StartDate)
    .ToListAsync();

   var vm = new StudentPaymentsVm
   {
    Agreements = agreements,
    Payments = agreements.SelectMany(a => a.Payments).OrderByDescending(p => p.DueDate).ToList()
   };
   return View("StudentIndex", vm);
  }

  var q = db.Payments.Include(x => x.RentalAgreement).ThenInclude(x => x.Property).AsQueryable();
  return View(await q.OrderByDescending(x => x.DueDate).ToListAsync());
 }
 [Authorize(Roles="Owner,Administrator")] public async Task<IActionResult> Record(int agreementId){return View(new PaymentVm{AgreementId=agreementId});}
 [HttpPost,Authorize(Roles="Owner,Administrator"),ValidateAntiForgeryToken] public async Task<IActionResult> Record(PaymentVm m){await payments.RecordAsync(m.AgreementId,m.Type,m.Amount,m.DueDate,m.Method);return RedirectToAction(nameof(Index));}}
