namespace Rental.ViewModels;

public class DashboardViewModel
{
    public int PropertyCount { get; set; }
    public int TenantCount { get; set; }
    public int ActiveLeaseCount { get; set; }
    public decimal TotalMonthlyRent { get; set; }
    public decimal PaymentsCollectedThisMonth { get; set; }
}
