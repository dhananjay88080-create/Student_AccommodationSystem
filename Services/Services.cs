using Microsoft.EntityFrameworkCore;
using StudentAccommodationSystem.Data;
using StudentAccommodationSystem.Models;
namespace StudentAccommodationSystem.Services;
public interface INotificationService { Task NotifyAsync(string userId, string message, string? url = null); }
public class NotificationService(ApplicationDbContext db) : INotificationService { public async Task NotifyAsync(string userId,string message,string? url=null){ db.Notifications.Add(new Notification{UserId=userId,Message=message,Url=url}); await db.SaveChangesAsync(); } }
public interface IBookingService { Task<(bool Ok,string Message)> ApproveAsync(int bookingId, string ownerUserId); }
public class BookingService(ApplicationDbContext db, INotificationService notifications) : IBookingService
{
 public async Task<(bool Ok,string Message)> ApproveAsync(int bookingId,string ownerUserId)
 {
  var booking=await db.Bookings.Include(x=>x.Property).ThenInclude(x=>x.Owner).Include(x=>x.Room).Include(x=>x.Student).ThenInclude(x=>x.User).FirstOrDefaultAsync(x=>x.Id==bookingId);
  if(booking is null) return (false,"Booking not found.");
  if(booking.Property.Owner.UserId!=ownerUserId) return (false,"You are not authorized to approve this booking.");
  if(booking.Status!=BookingStatus.Pending || booking.Room.Status!=RoomStatus.Available) return (false,"The booking is no longer eligible for approval.");
  var conflict=await db.Bookings.AnyAsync(x=>x.RoomId==booking.RoomId && x.Id!=booking.Id && x.Status==BookingStatus.Approved && booking.RequestedMoveIn < x.RequestedMoveOut && booking.RequestedMoveOut > x.RequestedMoveIn);
  if(conflict) return (false,"The room has a conflicting active booking.");
  booking.Status=BookingStatus.Approved; booking.Room.Status=RoomStatus.Occupied; booking.Room.CurrentOccupants=1;
  var agreement=new RentalAgreement{AgreementNumber=$"AGR-{DateTime.UtcNow:yyyyMMdd}-{booking.Id:D5}",BookingId=booking.Id,StudentProfileId=booking.StudentProfileId,OwnerProfileId=booking.Property.OwnerProfileId,PropertyId=booking.PropertyId,RoomId=booking.RoomId,StartDate=booking.RequestedMoveIn,EndDate=booking.RequestedMoveOut,MonthlyRent=booking.Room.MonthlyRent,SecurityDeposit=booking.Room.SecurityDeposit,TermsAndConditions="Keep the premises clean, pay rent by the due date, and report maintenance promptly."};
  db.RentalAgreements.Add(agreement); await db.SaveChangesAsync();
  await notifications.NotifyAsync(booking.Student.UserId,$"Your booking for {booking.Property.Name} was approved.","/Booking/MyBookings");
  return (true,"Booking approved and rental agreement created.");
 }
}
public interface IPaymentService { Task RecordAsync(int agreementId, PaymentType type, decimal amount, DateTime dueDate, string method); }
public class PaymentService(ApplicationDbContext db) : IPaymentService { public async Task RecordAsync(int agreementId,PaymentType type,decimal amount,DateTime dueDate,string method){ db.Payments.Add(new Payment{RentalAgreementId=agreementId,Type=type,Amount=amount,DueDate=dueDate,PaymentDate=DateTime.UtcNow,PaymentMethod=method,TransactionReference=$"DEMO-{Guid.NewGuid():N}"[..16],ReceiptNumber=$"RCPT-{DateTime.UtcNow:yyyyMMddHHmmss}",Status=PaymentStatus.Paid}); await db.SaveChangesAsync(); } }
