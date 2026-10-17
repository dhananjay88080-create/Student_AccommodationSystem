using System.ComponentModel.DataAnnotations;
namespace StudentAccommodationSystem.Models;
public class MaintenanceRequest
{
 public int Id { get; set; } public int StudentProfileId { get; set; } public int PropertyId { get; set; } public int RoomId { get; set; }
 [Required, StringLength(150)] public string Title { get; set; } = ""; [Required, StringLength(2000)] public string Description { get; set; } = ""; public Priority Priority { get; set; } = Priority.Medium; public string? ImageUrl { get; set; } public DateTime CreatedAt { get; set; } = DateTime.UtcNow; public MaintenanceStatus Status { get; set; } = MaintenanceStatus.Submitted;
 public Property Property { get; set; } = null!; public Room Room { get; set; } = null!;
}
