# EF Core migrations

The project is configured for Code First migrations. Because this source package was prepared in an environment without the .NET SDK, the generated binary migration snapshot is intentionally not fabricated.

Run:

```bash
dotnet ef migrations add InitialCreate
dotnet ef database update
```

After the first migration exists, `DbInitializer` applies the migration chain automatically on application startup.
