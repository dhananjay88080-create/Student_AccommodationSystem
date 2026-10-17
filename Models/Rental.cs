using System.ComponentModel.DataAnnotations;
namespace StudentAccommodationSystem.Models;
public class Booking
{
 public int Id { get; set; } public int StudentProfileId { get; set; } public int PropertyId { get; set; } public int RoomId { get; set; }
 public DateTime RequestedMoveIn { get; set; } public DateTime RequestedMoveOut { get; set; }
 [StringLength(1000)] public string? Message { get; set; } public BookingStatus Status { get; set; } = BookingStatus.Pending; public DateTime RequestDate { get; set; } = DateTime.UtcNow;
 public StudentProfile Student { get; set; } = null!; public Property Property { get; set; } = null!; public Room Room { get; set; } = null!; public RentalAgreement? RentalAgreement { get; set; }
}
public class RentalAgreement
{
 public int Id { get; set; } [Required] public string AgreementNumber { get; set; } = ""; public int BookingId { get; set; } public int StudentProfileId { get; set; } public int OwnerProfileId { get; set; } public int PropertyId { get; set; } public int RoomId { get; set; }
 public DateTime StartDate { get; set; } public DateTime EndDate { get; set; } public decimal MonthlyRent { get; set; } public decimal SecurityDeposit { get; set; } public int PaymentDueDay { get; set; } = 5; public AgreementStatus Status { get; set; } = AgreementStatus.Active;
 [Required] public string TermsAndConditions { get; set; } = "Standard rental terms apply.";
 public Booking Booking { get; set; } = null!; public StudentProfile Student { get; set; } = null!; public OwnerProfile Owner { get; set; } = null!; public Property Property { get; set; } = null!; public Room Room { get; set; } = null!; public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}
public class Payment
{
 public int Id { get; set; } public int RentalAgreementId { get; set; } public PaymentType Type { get; set; } public decimal Amount { get; set; } public DateTime? PaymentDate { get; set; } public DateTime DueDate { get; set; } public string? PaymentMethod { get; set; } public string? TransactionReference { get; set; } public PaymentStatus Status { get; set; } = PaymentStatus.Pending; public string? ReceiptNumber { get; set; } public RentalAgreement RentalAgreement { get; set; } = null!;
}
