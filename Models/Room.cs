using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
namespace StudentAccommodationSystem.Models;
public class Room
{
 public int Id { get; set; } public int PropertyId { get; set; }
 [Required, StringLength(50)] public string RoomNumber { get; set; } = "";
 [Required, StringLength(50)] public string RoomType { get; set; } = "Single";
 [Range(1,20)] public int Capacity { get; set; } = 1; public int CurrentOccupants { get; set; }
 [Range(0,1000000)] public decimal MonthlyRent { get; set; }
 public decimal SecurityDeposit { get; set; } public bool IsFurnished { get; set; }
 [StringLength(500)] public string? Amenities { get; set; } public RoomStatus Status { get; set; } = RoomStatus.Available;
 [ValidateNever] public Property Property { get; set; } = null!; public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
