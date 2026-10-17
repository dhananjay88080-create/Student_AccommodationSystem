using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using StudentAccommodationSystem.Models;
namespace StudentAccommodationSystem.Data;
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
 public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
 public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>(); public DbSet<OwnerProfile> OwnerProfiles => Set<OwnerProfile>(); public DbSet<Property> Properties => Set<Property>(); public DbSet<PropertyImage> PropertyImages => Set<PropertyImage>(); public DbSet<Amenity> Amenities => Set<Amenity>(); public DbSet<PropertyAmenity> PropertyAmenities => Set<PropertyAmenity>(); public DbSet<Room> Rooms => Set<Room>(); public DbSet<Booking> Bookings => Set<Booking>(); public DbSet<RentalAgreement> RentalAgreements => Set<RentalAgreement>(); public DbSet<Payment> Payments => Set<Payment>(); public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>(); public DbSet<Review> Reviews => Set<Review>(); public DbSet<Notification> Notifications => Set<Notification>(); public DbSet<MessageThread> MessageThreads => Set<MessageThread>(); public DbSet<Message> Messages => Set<Message>();
 protected override void OnModelCreating(ModelBuilder b)
 {
  base.OnModelCreating(b);
  b.Entity<StudentProfile>().HasIndex(x=>x.UserId).IsUnique(); b.Entity<OwnerProfile>().HasIndex(x=>x.UserId).IsUnique();
  b.Entity<OwnerProfile>().HasOne(x=>x.User).WithOne(x=>x.OwnerProfile).HasForeignKey<OwnerProfile>(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<StudentProfile>().HasOne(x=>x.User).WithOne(x=>x.StudentProfile).HasForeignKey<StudentProfile>(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Property>().HasOne(x=>x.Owner).WithMany(x=>x.Properties).HasForeignKey(x=>x.OwnerProfileId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<PropertyAmenity>().HasKey(x=>new { x.PropertyId, x.AmenityId });
  b.Entity<PropertyAmenity>().HasOne(x=>x.Property).WithMany(x=>x.PropertyAmenities).HasForeignKey(x=>x.PropertyId);
  b.Entity<PropertyAmenity>().HasOne(x=>x.Amenity).WithMany(x=>x.PropertyAmenities).HasForeignKey(x=>x.AmenityId);
  b.Entity<Booking>().HasOne(x=>x.Student).WithMany(x=>x.Bookings).HasForeignKey(x=>x.StudentProfileId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Booking>().HasOne(x=>x.Property).WithMany().HasForeignKey(x=>x.PropertyId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Booking>().HasOne(x=>x.Room).WithMany(x=>x.Bookings).HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<RentalAgreement>().HasOne(x=>x.Booking).WithOne(x=>x.RentalAgreement).HasForeignKey<RentalAgreement>(x=>x.BookingId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<RentalAgreement>().HasOne(x=>x.Student).WithMany().HasForeignKey(x=>x.StudentProfileId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<RentalAgreement>().HasOne(x=>x.Owner).WithMany().HasForeignKey(x=>x.OwnerProfileId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<RentalAgreement>().HasOne(x=>x.Property).WithMany().HasForeignKey(x=>x.PropertyId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<RentalAgreement>().HasOne(x=>x.Room).WithMany().HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Payment>().HasOne(x=>x.RentalAgreement).WithMany(x=>x.Payments).HasForeignKey(x=>x.RentalAgreementId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<MaintenanceRequest>().HasOne(x=>x.Property).WithMany().HasForeignKey(x=>x.PropertyId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<MaintenanceRequest>().HasOne(x=>x.Room).WithMany().HasForeignKey(x=>x.RoomId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Review>().HasOne(x=>x.Student).WithMany(x=>x.Reviews).HasForeignKey(x=>x.StudentProfileId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<Review>().HasOne(x=>x.Property).WithMany(x=>x.Reviews).HasForeignKey(x=>x.PropertyId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Message>().HasOne(x=>x.Sender).WithMany(x=>x.SentMessages).HasForeignKey(x=>x.SenderId).OnDelete(DeleteBehavior.Restrict);
  b.Entity<MessageThread>().HasMany(x=>x.Messages).WithOne(x=>x.Thread).HasForeignKey(x=>x.MessageThreadId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Notification>().HasOne(x=>x.User).WithMany(x=>x.Notifications).HasForeignKey(x=>x.UserId).OnDelete(DeleteBehavior.Cascade);
  b.Entity<Property>().Property(x=>x.SecurityDeposit).HasPrecision(18,2); b.Entity<Property>().Property(x=>x.Latitude).HasPrecision(9,6); b.Entity<Property>().Property(x=>x.Longitude).HasPrecision(9,6); b.Entity<Room>().Property(x=>x.MonthlyRent).HasPrecision(18,2); b.Entity<Room>().Property(x=>x.SecurityDeposit).HasPrecision(18,2); b.Entity<RentalAgreement>().Property(x=>x.MonthlyRent).HasPrecision(18,2); b.Entity<RentalAgreement>().Property(x=>x.SecurityDeposit).HasPrecision(18,2); b.Entity<Payment>().Property(x=>x.Amount).HasPrecision(18,2);
  b.Entity<Booking>().HasIndex(x=>new { x.RoomId, x.Status }); b.Entity<Payment>().HasIndex(x=>new { x.DueDate, x.Status });
 }
}
