# Rental Manager (ASP.NET Core)

Rental Manager is a full-featured ASP.NET Core MVC application designed to help landlords monitor properties, tenants, lease agreements, and rent collections in one place. It uses Entity Framework Core with SQL Server for durable persistence and seeds demo data on first launch so you can explore immediately.

## Features

- **Insightful dashboard** – Surface portfolio KPIs such as total properties, active leases, rent due, and payments collected with an upcoming expirations table.
- **Property directory** – Maintain addresses, unit counts, and notes while reviewing active leases per building.
- **Tenant CRM** – Store renter contact details and view their lease history in a single view.
- **Lease management** – Capture lease terms, rent amounts, and status with quick access to payments tied to each agreement.
- **Payment tracking** – Log monthly rent receipts by lease and method to keep books up to date.
- **Automatic sample data** – The database is created and populated the first time the app starts so you can interact with a realistic dataset instantly.

## Technology stack

- [ASP.NET Core MVC 8](https://learn.microsoft.com/aspnet/core) for the web application and Razor UI.
- [Entity Framework Core](https://learn.microsoft.com/ef/core) with the SQL Server provider for data access.
- Bootstrap 5 for layout and styling with a small amount of custom CSS.

## Getting started

1. **Install prerequisites**

   Install the [.NET 8 SDK](https://dotnet.microsoft.com/download) for your platform.

2. **Restore dependencies**

   ```bash
   dotnet restore
   ```

3. **Run the development server**

   ```bash
   dotnet run
   ```

   The application listens on the ASP.NET Core default port (usually <https://localhost:5001> with HTTPS and <http://localhost:5000> with HTTP).

4. **Explore the seeded portfolio**

   The first launch provisions a SQL Server database named `RentalDb` (LocalDB by default) and seeds a few properties, tenants, leases, and payments. You can add, edit, or delete records using the navigation links in the header. Update the `RentalDatabase` connection string in `appsettings.json` if you want to target a different SQL Server instance.

## Project structure

```
.
├── Controllers/             # MVC controllers for dashboard and CRUD areas
├── Data/                    # EF Core DbContext and seed logic
├── Models/                  # Entity classes for properties, tenants, leases, payments
├── ViewModels/              # Dashboard presentation models
├── Views/                   # Razor views and shared layout/partials
├── wwwroot/                 # Static assets (Bootstrap overrides)
├── Program.cs               # ASP.NET Core entry point and middleware configuration
├── Rental.csproj          # Project definition and package references
└── appsettings.json         # Configuration including SQL Server connection string
```

## Development notes

- Entity Framework Core uses `EnsureCreated` on startup; drop the `RentalDb` database (or clear your target SQL Server database) to reset the sample data.
- Validation attributes on the models drive both client-side and server-side validation for forms.
- The solution targets .NET 8 and uses SQL Server by default. Swap the connection string in `appsettings.json` and install another EF Core provider if you prefer SQLite, PostgreSQL, or a different database engine.
