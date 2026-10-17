/*
  ONE-TIME RESET: removes all property and room listings and their dependent
  booking/rental/payment/maintenance/review records so you can add listings manually.
  BACK UP YOUR DATABASE BEFORE RUNNING. This is destructive and cannot be undone.
*/
SET XACT_ABORT ON;
BEGIN TRANSACTION;

DELETE FROM dbo.Payments;
DELETE FROM dbo.RentalAgreements;
DELETE FROM dbo.MaintenanceRequests;
DELETE FROM dbo.Reviews;
DELETE FROM dbo.Bookings;
DELETE FROM dbo.PropertyAmenities;
DELETE FROM dbo.PropertyImages;
DELETE FROM dbo.Rooms;
DELETE FROM dbo.Properties;

COMMIT TRANSACTION;
