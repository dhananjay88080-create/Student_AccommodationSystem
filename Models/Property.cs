using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
namespace StudentAccommodationSystem.Models;
public class Property
{
 public int Id { get; set; }
 [Required, StringLength(150)] public string Name { get; set; } = "";
 [StringLength(2000)] public string? Description { get; set; }
 [Required, StringLength(250)] public string Address { get; set; } = "";
 [Required, StringLength(80)] public string City { get; set; } = "";
 [Required, StringLength(80)] public string State { get; set; } = "";
 [StringLength(20)] public string? PostalCode { get; set; }
 public decimal? Latitude { get; set; } public decimal? Longitude { get; set; }
 [Required, StringLength(50)] public string PropertyType { get; set; } = "Hostel";
 [Required] public int OwnerProfileId { get; set; }
 [StringLength(30)] public string? ContactPhone { get; set; }
 [StringLength(100)] public string? Rules { get; set; }
 public decimal SecurityDeposit { get; set; }
 [StringLength(30)] public string Status { get; set; } = "Pending";
 public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
 [ValidateNever] public OwnerProfile Owner { get; set; } = null!;
 public ICollection<PropertyImage> Images { get; set; } = new List<PropertyImage>();
 public ICollection<Room> Rooms { get; set; } = new List<Room>();
 public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>();
 public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
public class PropertyImage { public int Id { get; set; } public int PropertyId { get; set; } [Required] public string ImageUrl { get; set; } = ""; public bool IsPrimary { get; set; } public Property Property { get; set; } = null!; }
public class Amenity { public int Id { get; set; } [Required, StringLength(80)] public string Name { get; set; } = ""; public ICollection<PropertyAmenity> PropertyAmenities { get; set; } = new List<PropertyAmenity>(); }
public class PropertyAmenity { public int PropertyId { get; set; } public int AmenityId { get; set; } public Property Property { get; set; } = null!; public Amenity Amenity { get; set; } = null!; }
