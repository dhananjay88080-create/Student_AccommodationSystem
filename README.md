# Student Accommodation and Rental Management System

ASP.NET Core MVC 8 + EF Core + SQL Server + ASP.NET Core Identity + Bootstrap 5.

## Included
- Student, Owner/Landlord, and Administrator roles
- Property/room search and filtering
- Booking request and approval workflow
- Rental agreements
- Payment recording with rent/deposit separation
- Maintenance workflow
- Reviews and ratings
- Notifications and basic messaging model
- Role-specific dashboards
- Secure ownership checks, anti-forgery protection and validation
- EF Core Code First configuration and seed data

## Prerequisites
- .NET 8 SDK
- SQL Server / LocalDB

## Setup
```bash
dotnet restore
dotnet tool install --global dotnet-ef   # if not already installed
dotnet ef migrations add InitialCreate
dotnet ef database update
dotnet run
```
The seed routine also calls `Database.MigrateAsync()` so an existing migration chain is applied automatically at startup. For a fresh checkout, create the first migration using the command above.

## Demo credentials
- Admin: `admin@accommodation.local` / `Admin@12345`
- Owner: `owner@accommodation.local` / `Owner@12345`
- Student: `student@accommodation.local` / `Student@12345`

Change demo passwords before any real deployment.

## Connection string
Edit `appsettings.json` for your SQL Server instance.

## Architecture
`Models` contains the domain model; `Data` owns EF Core; `Services` contains workflow/business logic; `ViewModels` isolates UI input; `Controllers` handle HTTP concerns; Razor views provide the responsive Bootstrap UI.

## Workflow
Student searches -> selects room -> submits booking -> owner approves -> rental agreement is created -> rent/deposit payments are recorded -> maintenance/reviews are handled during/after the stay.

## ER relationship summary
ApplicationUser 1:1 StudentProfile/OwnerProfile. Owner 1:M Property. Property 1:M Room and PropertyImage. Property M:M Amenity through PropertyAmenity. Student 1:M Booking; Room 1:M Booking. Approved Booking 1:1 RentalAgreement. RentalAgreement 1:M Payment. Student/Room/Property 1:M MaintenanceRequest. Student 1:M Review and Property 1:M Review. ApplicationUser 1:M Notification and Message.

## Production hardening
Use real email verification, password reset email delivery, object storage for uploads, payment gateway tokens instead of card data, concurrency tokens for room inventory, audit logging, rate limiting, HTTPS-only cookies, and a managed secret store.

## Account enable/disable troubleshooting

The Admin Students and Owners pages should show the `Account controls v2` label. If it is missing, the running app is not using this source folder: stop the app, open this folder's `StudentAccommodationSystem.csproj`, rebuild, and run it again.

The account status action writes `ApplicationUser.IsActive` directly using the configured Entity Framework database context and reloads the row before displaying success. Check `appsettings.json` and confirm `DefaultConnection` points to the database you inspect in SSMS. The default in this project is `(localdb)\\MSSQLLocalDB`, database `StudentAccommodationDb`. Enabling an account sets `IsActive = true`, clears `LockoutEnd`, and resets failed login attempts. The login action checks `IsActive`.


## Property listings are no longer seeded

The startup initializer no longer creates demo properties or rooms. To remove listings already stored in an existing database, back up the database, then run `ClearPropertyData.sql` once in SQL Server Management Studio against the database configured in `appsettings.json`. This also deletes linked bookings, rental agreements, payments, maintenance requests, and reviews. Do not run it if you need to preserve those records. After cleanup, add your real properties manually from the Owner dashboard.
