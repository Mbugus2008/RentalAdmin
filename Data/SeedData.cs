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
                PhoneNumber = "+254 700 000 100",
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

        if (!await context.Invoices.AnyAsync())
        {
            await SeedInvoicesAsync(context, today);
        }

        if (!await context.AppSettings.AnyAsync())
        {
            context.AppSettings.Add(new AppSettings());
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedCorePortfolioAsync(RentalContext context, DateTime today)
    {
        var properties = new List<Property>
        {
            new()
            {
                Name = "Kilimani Heights",
                Address = "Wood Avenue, Kilimani",
                City = "Nairobi",
                State = "Nairobi County",
                ZipCode = "00100",
                Units = 12,
                Notes = "Modern mid-rise apartments with renovated kitchens."
            },
            new()
            {
                Name = "Nyali Palms",
                Address = "Links Road, Nyali",
                City = "Mombasa",
                State = "Mombasa County",
                ZipCode = "80100",
                Units = 8,
                Notes = "Beachside maisonettes a short walk from the reef."
            }
        };

        var tenants = new List<Tenant>
        {
            new()
            {
                FirstName = "Wanjiru",
                LastName = "Kamau",
                Email = "wanjiru.kamau@example.com",
                Phone = "+254 712 345 678",
                MoveInDate = today.AddMonths(-4)
            },
            new()
            {
                FirstName = "Otieno",
                LastName = "Odhiambo",
                Email = "otieno.odhiambo@example.com",
                Phone = "+254 722 987 654",
                MoveInDate = today.AddMonths(-14)
            },
            new()
            {
                FirstName = "Aisha",
                LastName = "Hassan",
                Email = "aisha.hassan@example.com",
                Phone = "+254 733 456 789",
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
                MonthlyRent = 85000m,
                SecurityDeposit = 85000m,
                Status = "Active"
            },
            new()
            {
                Property = properties[1],
                Tenant = tenants[1],
                StartDate = today.AddMonths(-14),
                EndDate = today.AddMonths(2),
                MonthlyRent = 95000m,
                SecurityDeposit = 95000m,
                Status = "Pending Renewal"
            },
            new()
            {
                Property = properties[0],
                Tenant = tenants[2],
                StartDate = today.AddMonths(-2),
                EndDate = today.AddMonths(10),
                MonthlyRent = 75000m,
                SecurityDeposit = 75000m,
                Status = "Active"
            }
        };

        var payments = new List<Payment>
        {
            new()
            {
                Lease = leases[0],
                PaymentDate = today.AddMonths(-2),
                Amount = 85000m,
                Method = "M-PESA"
            },
            new()
            {
                Lease = leases[0],
                PaymentDate = today.AddMonths(-1),
                Amount = 85000m,
                Method = "M-PESA"
            },
            new()
            {
                Lease = leases[1],
                PaymentDate = today.AddMonths(-1).AddDays(2),
                Amount = 95000m,
                Method = "Cheque"
            },
            new()
            {
                Lease = leases[2],
                PaymentDate = today.AddMonths(-1),
                Amount = 75000m,
                Method = "M-PESA"
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
                Amount = 85000m,
                Category = "Rent",
                Type = "Income",
                PropertyId = propertyId,
                TenantId = tenantId,
                Description = "Monthly rent payment"
            },
            new()
            {
                TransactionDate = today.AddMonths(-1),
                Amount = 85000m,
                Category = "Rent",
                Type = "Income",
                PropertyId = propertyId,
                TenantId = tenantId,
                Description = "Monthly rent payment"
            },
            new()
            {
                TransactionDate = today.AddDays(-12),
                Amount = 12500m,
                Category = "Maintenance",
                Type = "Expense",
                PropertyId = propertyId,
                Description = "Plumbing repair"
            },
            new()
            {
                TransactionDate = today.AddDays(-5),
                Amount = 4500m,
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
                EstimatedCost = 8500m,
                RequestDate = today.AddDays(-6)
            },
            new()
            {
                Title = "Air conditioner not cooling",
                Description = "Air conditioner runs but does not cool the apartment.",
                PropertyId = properties[0].Id,
                UnitId = 7,
                Priority = "High",
                Status = "In Progress",
                AssignedTo = "CoolAir Kenya",
                EstimatedCost = 25000m,
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
                AssignedTo = "Nyali Locksmiths",
                EstimatedCost = 10000m,
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

    private static async Task SeedInvoicesAsync(RentalContext context, DateTime today)
    {
        var tenants = await context.Tenants.OrderBy(t => t.Id).ToListAsync();
        var properties = await context.Properties.OrderBy(p => p.Id).ToListAsync();
        if (tenants.Count == 0 || properties.Count == 0)
        {
            return;
        }

        var tenant = tenants[0]; // Demo: first tenant (Wanjiru Kamau)
        var property = properties[0]; // Demo: first property (Kilimani Heights)

        var invoices = new List<Invoice>
        {
            new()
            {
                InvoiceNumber = "INV-0001",
                TenantId = tenant.Id,
                PropertyId = property.Id,
                InvoiceDate = today.AddMonths(-2),
                DueDate = today.AddMonths(-2).AddDays(5),
                Status = "Paid",
                AmountPaid = 88000m,
                TotalAmount = 88000m,
                Notes = "Rent, water and garbage.",
                Lines = new List<InvoiceLine>
                {
                    new() { Description = "Rent", Amount = 85000m },
                    new() { Description = "Water", Amount = 2500m },
                    new() { Description = "Garbage", Amount = 500m }
                }
            },
            new()
            {
                InvoiceNumber = "INV-0002",
                TenantId = tenant.Id,
                PropertyId = property.Id,
                InvoiceDate = today.AddMonths(-1),
                DueDate = today.AddMonths(-1).AddDays(5),
                Status = "Paid",
                AmountPaid = 87800m,
                TotalAmount = 87800m,
                Notes = "Rent, water and garbage.",
                Lines = new List<InvoiceLine>
                {
                    new() { Description = "Rent", Amount = 85000m },
                    new() { Description = "Water", Amount = 2300m },
                    new() { Description = "Garbage", Amount = 500m }
                }
            },
            new()
            {
                InvoiceNumber = "INV-0003",
                TenantId = tenant.Id,
                PropertyId = property.Id,
                InvoiceDate = today,
                DueDate = today.AddDays(5),
                Status = "Sent",
                AmountPaid = 0m,
                TotalAmount = 88200m,
                Notes = "Rent, water and garbage.",
                Lines = new List<InvoiceLine>
                {
                    new() { Description = "Rent", Amount = 85000m },
                    new() { Description = "Water", Amount = 2700m },
                    new() { Description = "Garbage", Amount = 500m }
                }
            }
        };

        context.Invoices.AddRange(invoices);
        await context.SaveChangesAsync();
    }
}
