using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rental.Models;
using Rental.Services;

namespace Rental.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(RentalContext context)
    {
        var today = DateTime.Today;

        if (!await context.Properties.AnyAsync())
        {
            await SeedCorePortfolioAsync(context, today);
        }

        if (!await context.Users.AnyAsync())
        {
            context.Users.Add(new User
            {
                FirstName = "Demo",
                LastName = "Landlord",
                Email = "admin@rental.local",
                PhoneNumber = "555-0100",
                Role = "Landlord",
                PasswordHash = PasswordHasher.Hash("Admin123!"),
                CreatedAt = DateTime.UtcNow
            });
            await context.SaveChangesAsync();
        }

        if (!await context.Transactions.AnyAsync())
        {
            await SeedTransactionsAsync(context, today);
        }

        if (!await context.MaintenanceRequests.AnyAsync())
        {
            await SeedMaintenanceAsync(context, today);
        }

        if (!await context.Documents.AnyAsync())
        {
            await SeedDocumentsAsync(context, today);
        }
    }

    private static async Task SeedCorePortfolioAsync(RentalContext context, DateTime today)
    {
        var properties = new List<Property>
        {
            new()
            {
                Name = "Maple Apartments",
                Address = "742 Evergreen Terrace",
                City = "Springfield",
                State = "IL",
                ZipCode = "62704",
                Units = 12,
                Notes = "Downtown mid-rise with renovated kitchens."
            },
            new()
            {
                Name = "Lakeside Villas",
                Address = "19 Harbor View Rd",
                City = "Madison",
                State = "WI",
                ZipCode = "53703",
                Units = 8,
                Notes = "Townhome community near the lake trail."
            }
        };

        var tenants = new List<Tenant>
        {
            new()
            {
                FirstName = "Alicia",
                LastName = "Gomez",
                Email = "alicia.gomez@example.com",
                Phone = "555-0110",
                MoveInDate = today.AddMonths(-4)
            },
            new()
            {
                FirstName = "Marcus",
                LastName = "Lee",
                Email = "marcus.lee@example.com",
                Phone = "555-0142",
                MoveInDate = today.AddMonths(-14)
            },
            new()
            {
                FirstName = "Priya",
                LastName = "Shah",
                Email = "priya.shah@example.com",
                Phone = "555-0188",
                MoveInDate = today.AddMonths(-2)
            }
        };

        var leases = new List<Lease>
        {
            new()
            {
                Property = properties[0],
                Tenant = tenants[0],
                StartDate = today.AddMonths(-4),
                EndDate = today.AddMonths(8),
                MonthlyRent = 1650m,
                Status = "Active"
            },
            new()
            {
                Property = properties[1],
                Tenant = tenants[1],
                StartDate = today.AddMonths(-14),
                EndDate = today.AddMonths(2),
                MonthlyRent = 2100m,
                Status = "Pending Renewal"
            },
            new()
            {
                Property = properties[0],
                Tenant = tenants[2],
                StartDate = today.AddMonths(-2),
                EndDate = today.AddMonths(10),
                MonthlyRent = 1750m,
                Status = "Active"
            }
        };

        var payments = new List<Payment>
        {
            new()
            {
                Lease = leases[0],
                PaymentDate = today.AddMonths(-2),
                Amount = 1650m,
                Method = "Bank Transfer"
            },
            new()
            {
                Lease = leases[0],
                PaymentDate = today.AddMonths(-1),
                Amount = 1650m,
                Method = "Bank Transfer"
            },
            new()
            {
                Lease = leases[1],
                PaymentDate = today.AddMonths(-1).AddDays(2),
                Amount = 2100m,
                Method = "Check"
            },
            new()
            {
                Lease = leases[2],
                PaymentDate = today.AddMonths(-1),
                Amount = 1750m,
                Method = "ACH"
            }
        };

        await context.Properties.AddRangeAsync(properties);
        await context.Tenants.AddRangeAsync(tenants);
        await context.Leases.AddRangeAsync(leases);
        await context.Payments.AddRangeAsync(payments);

        await context.SaveChangesAsync();
    }

    private static async Task SeedTransactionsAsync(RentalContext context, DateTime today)
    {
        var properties = await context.Properties.OrderBy(p => p.Id).ToListAsync();
        var tenants = await context.Tenants.OrderBy(t => t.Id).ToListAsync();
        if (properties.Count == 0)
        {
            return;
        }

        var propertyId = properties[0].Id;
        var tenantId = tenants.FirstOrDefault()?.Id;

        var transactions = new List<Transaction>
        {
            new()
            {
                TransactionDate = today.AddMonths(-2),
                Amount = 1650m,
                Category = "Rent",
                Type = "Income",
                PropertyId = propertyId,
                TenantId = tenantId,
                Description = "Monthly rent payment"
            },
            new()
            {
                TransactionDate = today.AddMonths(-1),
                Amount = 1650m,
                Category = "Rent",
                Type = "Income",
                PropertyId = propertyId,
                TenantId = tenantId,
                Description = "Monthly rent payment"
            },
            new()
            {
                TransactionDate = today.AddDays(-12),
                Amount = 320m,
                Category = "Maintenance",
                Type = "Expense",
                PropertyId = propertyId,
                Description = "Plumbing repair"
            },
            new()
            {
                TransactionDate = today.AddDays(-5),
                Amount = 85m,
                Category = "Utilities",
                Type = "Expense",
                PropertyId = propertyId,
                Description = "Water bill"
            }
        };

        context.Transactions.AddRange(transactions);
        await context.SaveChangesAsync();
    }

    private static async Task SeedMaintenanceAsync(RentalContext context, DateTime today)
    {
        var properties = await context.Properties.OrderBy(p => p.Id).ToListAsync();
        if (properties.Count == 0)
        {
            return;
        }

        var requests = new List<MaintenanceRequest>
        {
            new()
            {
                Title = "Leaking kitchen faucet",
                Description = "Kitchen faucet drips constantly; needs a new cartridge.",
                PropertyId = properties[0].Id,
                UnitId = 3,
                Priority = "Medium",
                Status = "Open",
                EstimatedCost = 120m,
                RequestDate = today.AddDays(-6)
            },
            new()
            {
                Title = "HVAC not cooling",
                Description = "Air conditioner runs but does not cool the apartment.",
                PropertyId = properties[0].Id,
                UnitId = 7,
                Priority = "High",
                Status = "In Progress",
                AssignedTo = "CoolAir Services",
                EstimatedCost = 450m,
                RequestDate = today.AddDays(-2)
            },
            new()
            {
                Title = "Front door lock replacement",
                Description = "Front door lock sticks and the key is hard to turn.",
                PropertyId = properties[Math.Min(1, properties.Count - 1)].Id,
                UnitId = 1,
                Priority = "Low",
                Status = "Completed",
                AssignedTo = "Locksmith Pro",
                EstimatedCost = 200m,
                RequestDate = today.AddDays(-20),
                CompletedDate = today.AddDays(-14)
            }
        };

        context.MaintenanceRequests.AddRange(requests);
        await context.SaveChangesAsync();
    }

    private static async Task SeedDocumentsAsync(RentalContext context, DateTime today)
    {
        var properties = await context.Properties.OrderBy(p => p.Id).ToListAsync();
        var tenants = await context.Tenants.OrderBy(t => t.Id).ToListAsync();
        if (properties.Count == 0)
        {
            return;
        }

        var documents = new List<RentalDocument>
        {
            new()
            {
                Title = "Building Insurance Policy",
                Description = "Annual property insurance for the main building.",
                Category = "Insurance",
                PropertyId = properties[0].Id,
                ExpirationDate = today.AddMonths(6),
                UploadDate = today.AddMonths(-6),
                FileType = "PDF",
                FileSize = 245_000,
                FilePath = "/uploads/insurance-policy.pdf"
            },
            new()
            {
                Title = "Signed Lease - Unit 3",
                Description = "Executed lease agreement for unit 3.",
                Category = "Lease",
                PropertyId = properties[0].Id,
                TenantId = tenants.FirstOrDefault()?.Id,
                UploadDate = today.AddMonths(-4),
                FileType = "PDF",
                FileSize = 180_000,
                FilePath = "/uploads/lease-unit-3.pdf"
            }
        };

        context.Documents.AddRange(documents);
        await context.SaveChangesAsync();
    }
}
