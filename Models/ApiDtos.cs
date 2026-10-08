using System;
using Rental.Models;

namespace Rental.Api;

// ---------------------------------------------------------------
// Request DTOs — property names match the JSON keys sent by the
// Flutter app (camelCase by default with System.Text.Json).
// ---------------------------------------------------------------

public class LoginRequest
{
    public string? Email { get; set; }
    public string? Password { get; set; }
}

public class RegisterRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Phone { get; set; }
    public string? Password { get; set; }
}

public class PropertyRequest
{
    public string? Name { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? ZipCode { get; set; }
    public string? Country { get; set; }
    public string? PropertyType { get; set; }
    public int? TotalUnits { get; set; }
    public decimal? PurchasePrice { get; set; }
    public decimal? MarketValue { get; set; }
    public string? Description { get; set; }
}

public class TenantRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public string? PhoneNumber { get; set; }
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }
    public string? Notes { get; set; }
}

public class LeaseRequest
{
    public int? UnitId { get; set; }
    public int? TenantId { get; set; }
    public int? PropertyId { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public decimal? MonthlyRent { get; set; }
    public decimal? SecurityDeposit { get; set; }
    public string? LeaseTerms { get; set; }
    public string? Status { get; set; }
}

public class TransactionRequest
{
    public DateTime? TransactionDate { get; set; }
    public decimal? Amount { get; set; }
    public string? Category { get; set; }
    public string? Type { get; set; }
    public int? PropertyId { get; set; }
    public int? UnitId { get; set; }
    public int? TenantId { get; set; }
    public string? Description { get; set; }
}

public class MaintenanceRequestDto
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public int? PropertyId { get; set; }
    public int? UnitId { get; set; }
    public string? Priority { get; set; }
    public string? Status { get; set; }
    public string? AssignedTo { get; set; }
    public decimal? EstimatedCost { get; set; }
    public DateTime? RequestDate { get; set; }
    public DateTime? CompletedDate { get; set; }
}

public class DocumentRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }
    public string? Category { get; set; }
    public int? PropertyId { get; set; }
    public int? UnitId { get; set; }
    public int? TenantId { get; set; }
    public DateTime? ExpirationDate { get; set; }
    public DateTime? UploadDate { get; set; }
    public string? FileType { get; set; }
    public int? FileSize { get; set; }
    public string? FilePath { get; set; }
}

public class ExportReportRequest
{
    public string? ReportType { get; set; }
}

// ---------------------------------------------------------------
// Response mappers — produce the exact JSON shape the Flutter app
// models expect (see lib/app/core/models/*.dart).
// ---------------------------------------------------------------

public static class ApiMap
{
    public static object Property(Models.Property p) => new
    {
        id = p.Id,
        name = p.Name,
        address = p.Address,
        city = p.City,
        state = p.State,
        zipCode = p.ZipCode,
        country = p.Country,
        propertyType = p.PropertyType,
        totalUnits = p.Units,
        purchasePrice = p.PurchasePrice,
        marketValue = p.MarketValue,
        description = p.Notes,
        createdAt = p.CreatedAt,
        updatedAt = p.UpdatedAt
    };

    public static object Tenant(Models.Tenant t) => new
    {
        id = t.Id,
        firstName = t.FirstName,
        lastName = t.LastName,
        email = t.Email,
        phoneNumber = t.Phone,
        emergencyContactName = t.EmergencyContactName,
        emergencyContactPhone = t.EmergencyContactPhone,
        notes = t.Notes,
        createdAt = t.CreatedAt,
        updatedAt = t.UpdatedAt
    };

    public static object Lease(Models.Lease l) => new
    {
        id = l.Id,
        unitId = l.UnitId,
        tenantId = l.TenantId,
        propertyId = l.PropertyId,
        startDate = l.StartDate,
        endDate = l.EndDate,
        monthlyRent = l.MonthlyRent,
        securityDeposit = l.SecurityDeposit,
        leaseTerms = l.LeaseTerms,
        status = l.Status,
        createdAt = l.CreatedAt,
        updatedAt = l.UpdatedAt
    };

    public static object Transaction(Models.Transaction t) => new
    {
        id = t.Id,
        transactionDate = t.TransactionDate,
        amount = t.Amount,
        category = t.Category,
        type = t.Type,
        propertyId = t.PropertyId,
        unitId = t.UnitId,
        tenantId = t.TenantId,
        description = t.Description,
        createdAt = t.CreatedAt,
        updatedAt = t.UpdatedAt
    };

    public static object Maintenance(Models.MaintenanceRequest m) => new
    {
        id = m.Id,
        title = m.Title,
        description = m.Description,
        propertyId = m.PropertyId,
        unitId = m.UnitId,
        priority = m.Priority,
        status = m.Status,
        assignedTo = m.AssignedTo,
        estimatedCost = m.EstimatedCost,
        requestDate = m.RequestDate,
        completedDate = m.CompletedDate,
        createdAt = m.CreatedAt,
        updatedAt = m.UpdatedAt
    };

    public static object Document(Models.RentalDocument d) => new
    {
        id = d.Id,
        title = d.Title,
        description = d.Description,
        category = d.Category,
        propertyId = d.PropertyId,
        unitId = d.UnitId,
        tenantId = d.TenantId,
        expirationDate = d.ExpirationDate,
        uploadDate = d.UploadDate,
        fileType = d.FileType,
        fileSize = d.FileSize,
        filePath = d.FilePath,
        createdAt = d.CreatedAt,
        updatedAt = d.UpdatedAt
    };

    public static object User(Models.User u) => new
    {
        id = u.Id,
        firstName = u.FirstName,
        lastName = u.LastName,
        email = u.Email,
        phoneNumber = u.PhoneNumber,
        role = u.Role,
        createdAt = u.CreatedAt
    };
}
