using Microsoft.EntityFrameworkCore;
using Rental.Models;

namespace Rental.Data;

public class RentalContext : DbContext
{
    public RentalContext(DbContextOptions<RentalContext> options) : base(options)
    {
    }

    public DbSet<Property> Properties => Set<Property>();
    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<Lease> Leases => Set<Lease>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<MaintenanceRequest> MaintenanceRequests => Set<MaintenanceRequest>();
    public DbSet<RentalDocument> Documents => Set<RentalDocument>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Property>()
            .HasMany(p => p.Leases)
            .WithOne(l => l.Property!)
            .HasForeignKey(l => l.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Tenant>()
            .HasMany(t => t.Leases)
            .WithOne(l => l.Tenant!)
            .HasForeignKey(l => l.TenantId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Lease>()
            .HasMany(l => l.Payments)
            .WithOne(p => p.Lease!)
            .HasForeignKey(p => p.LeaseId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Property)
            .WithMany()
            .HasForeignKey(t => t.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.Tenant)
            .WithMany()
            .HasForeignKey(t => t.TenantId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<MaintenanceRequest>()
            .HasOne(m => m.Property)
            .WithMany()
            .HasForeignKey(m => m.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RentalDocument>()
            .HasOne(d => d.Property)
            .WithMany()
            .HasForeignKey(d => d.PropertyId)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<RentalDocument>()
            .HasOne(d => d.Tenant)
            .WithMany()
            .HasForeignKey(d => d.TenantId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
