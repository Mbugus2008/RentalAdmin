using System;
using System.Collections.Generic;

namespace Rental.ViewModels;

public class ReportsViewModel
{
    // Financials
    public decimal TotalRevenue { get; set; }
    public decimal TotalExpenses { get; set; }
    public decimal NetIncome => TotalRevenue - TotalExpenses;

    // Portfolio
    public int PropertiesCount { get; set; }
    public int UnitsCount { get; set; }
    public int ActiveLeaseCount { get; set; }
    public double OccupancyRate { get; set; }

    // Invoicing
    public decimal InvoicedTotal { get; set; }
    public decimal CollectedTotal { get; set; }
    public decimal OutstandingTotal => InvoicedTotal - CollectedTotal;

    public List<CategoryTotal> IncomeByCategory { get; set; } = new();
    public List<CategoryTotal> ExpenseByCategory { get; set; } = new();
    public List<TenantBalance> TenantBalances { get; set; } = new();
    public List<ExpiringLease> ExpiringLeases { get; set; } = new();

    public class CategoryTotal
    {
        public string Category { get; set; } = string.Empty;
        public decimal Amount { get; set; }
    }

    public class TenantBalance
    {
        public string TenantName { get; set; } = string.Empty;
        public decimal Invoiced { get; set; }
        public decimal Paid { get; set; }
        public decimal Balance { get; set; }
    }

    public class ExpiringLease
    {
        public string PropertyName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
