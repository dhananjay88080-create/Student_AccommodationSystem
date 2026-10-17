using System.ComponentModel.DataAnnotations;
using StudentAccommodationSystem.Models;
namespace StudentAccommodationSystem.ViewModels;
public class RegisterViewModel { [Required] public string FullName{get;set;}=""; [Required,EmailAddress] public string Email{get;set;}=""; [Required,DataType(DataType.Password),MinLength(8)] public string Password{get;set;}=""; [Required] public string Role{get;set;}="Student"; }
public class LoginViewModel { [Required,EmailAddress] public string Email{get;set;}=""; [Required,DataType(DataType.Password)] public string Password{get;set;}=""; public bool RememberMe{get;set;} }
public class PropertySearchVm { public string? Location{get;set;} public string? Name{get;set;} public decimal? MinRent{get;set;} public decimal? MaxRent{get;set;} public string? RoomType{get;set;} public bool? Furnished{get;set;} public string? Amenity{get;set;} public List<Property> Results{get;set;}=new(); }
public class BookingVm { public int RoomId{get;set;} [DataType(DataType.Date)] public DateTime MoveIn{get;set;}=DateTime.Today.AddDays(7); [DataType(DataType.Date)] public DateTime MoveOut{get;set;}=DateTime.Today.AddMonths(6); [StringLength(1000)] public string? Message{get;set;} }
public class DashboardVm { public List<Property> OwnerProperties{get;set;}=new(); public int TotalProperties{get;set;} public int TotalRooms{get;set;} public int AvailableRooms{get;set;} public int OccupiedRooms{get;set;} public int PendingBookings{get;set;} public int ActiveTenants{get;set;} public decimal MonthlyIncome{get;set;} public int PendingMaintenance{get;set;} public int TotalStudents{get;set;} public int TotalOwners{get;set;} public int PendingApprovals{get;set;} public int ActiveAgreements{get;set;} public decimal TotalPayments{get;set;} public List<Payment> Payments{get;set;}=new(); }
public class PaymentVm { public int AgreementId{get;set;} public PaymentType Type{get;set;} public decimal Amount{get;set;} [DataType(DataType.Date)] public DateTime DueDate{get;set;}=DateTime.Today; public string Method{get;set;}="Demo Gateway"; }

public class StudentPaymentsVm
{
    public List<RentalAgreement> Agreements { get; set; } = new();
    public List<Payment> Payments { get; set; } = new();
}
