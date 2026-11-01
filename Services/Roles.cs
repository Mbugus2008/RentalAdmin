namespace Rental.Services;

/// <summary>
/// Staff user types shared by the admin console and the mobile app.
/// Stored in Users.Role as a plain string (no schema changes).
/// </summary>
public static class Roles
{
    public const string Admin = "Admin";
    public const string Owner = "Owner";
    public const string Finance = "Finance";
    public const string Agent = "Agent";
    public const string Maintenance = "Maintenance";

    /// <summary>Legacy role string used before user types existed — treated as Admin.</summary>
    public const string LegacyAdmin = "Landlord";

    public static readonly string[] All = { Admin, Owner, Finance, Agent, Maintenance };

    // Constant role lists for [Authorize(Roles = ...)] attributes.
    public const string AdminArea = Admin + "," + LegacyAdmin;
    public const string DashboardArea = AdminArea + "," + Owner + "," + Finance + "," + Agent;
    public const string PortfolioArea = AdminArea + "," + Owner + "," + Agent;
    public const string FinanceArea = AdminArea + "," + Owner + "," + Finance + "," + Agent;
    public const string LedgerArea = AdminArea + "," + Owner + "," + Finance;
    public const string MaintenanceArea = AdminArea + "," + Owner + "," + Agent + "," + Maintenance;

    /// <summary>Maps legacy/unknown values to the current vocabulary.</summary>
    public static string Normalize(string? role)
    {
        if (string.IsNullOrWhiteSpace(role))
        {
            return Admin;
        }

        return string.Equals(role, LegacyAdmin, System.StringComparison.OrdinalIgnoreCase)
            ? Admin
            : role;
    }

    public static bool IsValid(string? role) =>
        role is not null &&
        (All.Contains(role) || string.Equals(role, LegacyAdmin, System.StringComparison.OrdinalIgnoreCase));
}
