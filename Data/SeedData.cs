using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Data;

public static class SeedData
{
    public static async Task EnsureSeededAsync(RentalContext context)
    {
        if (await context.Properties.AnyAsync())
        {
            return;
        }

        var today = DateTime.Today;

        var properties = new List<Property>
        {
            new()
            {
                Name = "Maple Apartments",
                Address = "742 Evergreen Terrace",
                City = "Springfield",
                State = "IL",
                ZipCode = "62704",
                TotalUnits = 3,
                Notes = "Downtown mid-rise with renovated kitchens."
            },
            new()
            {
                Name = "Lakeside Villas",
                Address = "19 Harbor View Rd",
                City = "Madison",
                State = "WI",
                ZipCode = "53703",
                TotalUnits = 2,
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

        var units = new List<Unit>
        {
            new()
            {
                Property = properties[0],
                UnitType = "1 Bedroom"
            },
            new()
            {
                Property = properties[0],
                UnitType = "2 Bedroom"
            },
            new()
            {
                Property = properties[0],
                UnitType = "Studio"
            },
            new()
            {
                Property = properties[1],
                UnitType = "3 Bedroom"
            },
            new()
            {
                Property = properties[1],
                UnitType = "2 Bedroom"
            }
        };

        await context.Properties.AddRangeAsync(properties);
        await context.Tenants.AddRangeAsync(tenants);
        await context.Leases.AddRangeAsync(leases);
        await context.Payments.AddRangeAsync(payments);
        await context.Units.AddRangeAsync(units);

        await context.SaveChangesAsync();
    }
}
