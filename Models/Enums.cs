namespace StudentAccommodationSystem.Models;
public enum RoomStatus { Available, Reserved, Occupied, UnderMaintenance }
public enum BookingStatus { Pending, Approved, Rejected, Cancelled, Completed }
public enum AgreementStatus { Draft, Active, Expired, Terminated }
public enum PaymentType { Rent, SecurityDeposit }
public enum PaymentStatus { Pending, Paid, Failed, Overdue, Refunded }
public enum MaintenanceStatus { Submitted, Assigned, InProgress, Resolved, Closed, Rejected }
public enum Priority { Low, Medium, High, Urgent }
