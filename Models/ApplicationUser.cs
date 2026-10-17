using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
namespace StudentAccommodationSystem.Models;
public class ApplicationUser : IdentityUser
{
    [Required, StringLength(100)] public string FullName { get; set; } = "";
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public StudentProfile? StudentProfile { get; set; }
    public OwnerProfile? OwnerProfile { get; set; }
    public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
}
public class StudentProfile { public int Id { get; set; } public string UserId { get; set; } = ""; public string? StudentNumber { get; set; } public string? University { get; set; } public ApplicationUser User { get; set; } = null!; public ICollection<Booking> Bookings { get; set; } = new List<Booking>(); public ICollection<Review> Reviews { get; set; } = new List<Review>(); }
public class OwnerProfile { public int Id { get; set; } public string UserId { get; set; } = ""; public string? BusinessName { get; set; } public ApplicationUser User { get; set; } = null!; public ICollection<Property> Properties { get; set; } = new List<Property>(); }
