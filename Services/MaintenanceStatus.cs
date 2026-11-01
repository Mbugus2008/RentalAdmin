using System;

namespace Rental.Services;

/// <summary>Maintenance workflow statuses, shared by the web console and the app.</summary>
public static class MaintenanceStatus
{
    public const string Open = "Open";                 // logged, nobody assigned yet
    public const string Assigned = "Assigned";         // fundi/contractor allocated
    public const string InProgress = "In Progress";    // work started
    public const string Pending = "Pending";           // waiting on parts / owner approval
    public const string Completed = "Completed";       // fixed
    public const string Cancelled = "Cancelled";       // not done / abandoned

    public static readonly string[] All =
    {
        Open, Assigned, InProgress, Pending, Completed, Cancelled
    };

    public static bool IsClosed(string? status) =>
        string.Equals(status, Completed, StringComparison.OrdinalIgnoreCase)
        || string.Equals(status, Cancelled, StringComparison.OrdinalIgnoreCase);

    /// <summary>Soft badge CSS class for a status (admin console).</summary>
    public static string BadgeClass(string? status) => status?.ToLowerInvariant() switch
    {
        "open" => "badge-soft-brand",
        "assigned" => "badge-soft-info",
        "in progress" => "badge-soft-warning",
        "pending" => "badge-soft-purple",
        "completed" => "badge-soft-success",
        "cancelled" => "badge-soft-neutral",
        _ => "badge-soft-neutral"
    };
}
