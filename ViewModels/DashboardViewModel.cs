using System;
using System.Collections.Generic;

namespace Rental.ViewModels;

public class DashboardViewModel
{
    public int PropertyCount { get; set; }
    public int ActiveLeaseCount { get; set; }
    public decimal TotalMonthlyRent { get; set; }
    public decimal PaymentsCollectedThisMonth { get; set; }
    public List<LeaseSummary> UpcomingRenewals { get; set; } = new();

    public class LeaseSummary
    {
        public string PropertyName { get; set; } = string.Empty;
        public string TenantName { get; set; } = string.Empty;
        public DateTime? EndDate { get; set; }
        public decimal MonthlyRent { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
