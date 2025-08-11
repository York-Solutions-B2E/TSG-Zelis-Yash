using Microsoft.EntityFrameworkCore;
using CommunicationLifecycle.Core.Entities;

namespace CommunicationLifecycle.Infrastructure.Data;

public class CommunicationDbContext : DbContext
{
    public CommunicationDbContext(DbContextOptions<CommunicationDbContext> options) : base(options)
    {
    }

    public DbSet<Communication> Communications { get; set; }
    public DbSet<CommunicationStatusHistory> CommunicationStatusHistory { get; set; }
    public DbSet<CommunicationType> CommunicationTypes { get; set; }
    public DbSet<CommunicationTypeStatus> CommunicationTypeStatuses { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Communication entity configuration
        modelBuilder.Entity<Communication>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.TypeCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CurrentStatus).IsRequired().HasMaxLength(50);
            entity.Property(e => e.LastUpdatedUtc).IsRequired();
            entity.Property(e => e.CreatedUtc).IsRequired();
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.SourceFileUrl).HasMaxLength(200);

            entity.HasIndex(e => e.TypeCode);
            entity.HasIndex(e => e.CurrentStatus);
            entity.HasIndex(e => e.LastUpdatedUtc);
        });

        // CommunicationStatusHistory entity configuration
        modelBuilder.Entity<CommunicationStatusHistory>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.StatusCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.OccurredUtc).IsRequired();
            entity.Property(e => e.Notes).HasMaxLength(500);

            entity.HasOne(e => e.Communication)
                  .WithMany(c => c.StatusHistory)
                  .HasForeignKey(e => e.CommunicationId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(e => e.CommunicationId);
            entity.HasIndex(e => e.StatusCode);
            entity.HasIndex(e => e.OccurredUtc);
        });

        // CommunicationType entity configuration
        modelBuilder.Entity<CommunicationType>(entity =>
        {
            entity.HasKey(e => e.TypeCode);
            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.DisplayName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.IsActive).IsRequired().HasDefaultValue(true);
        });

        // CommunicationTypeStatus entity configuration
        modelBuilder.Entity<CommunicationTypeStatus>(entity =>
        {
            entity.HasKey(e => new { e.TypeCode, e.StatusCode });
            entity.Property(e => e.TypeCode).HasMaxLength(50);
            entity.Property(e => e.StatusCode).HasMaxLength(50);
            entity.Property(e => e.Description).HasMaxLength(500);
            entity.Property(e => e.DisplayOrder).IsRequired();

            entity.HasOne(e => e.CommunicationType)
                  .WithMany(ct => ct.TypeStatuses)
                  .HasForeignKey(e => e.TypeCode)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed initial data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Communication Types
        modelBuilder.Entity<CommunicationType>().HasData(
            new CommunicationType { TypeCode = "EOB", DisplayName = "Explanation of Benefits", Description = "Explanation of Benefits documents", IsActive = true },
            new CommunicationType { TypeCode = "EOP", DisplayName = "Explanation of Payment", Description = "Explanation of Payment documents", IsActive = true },
            new CommunicationType { TypeCode = "ID_CARD", DisplayName = "Member ID Card", Description = "Member identification cards", IsActive = true },
            new CommunicationType { TypeCode = "WELCOME_PACKET", DisplayName = "Welcome Packet", Description = "New member welcome packets", IsActive = true },
            new CommunicationType { TypeCode = "CLAIM_STATEMENT", DisplayName = "Claim Statement", Description = "Claim statements", IsActive = true },
            new CommunicationType { TypeCode = "PROVIDER_STATEMENT", DisplayName = "Provider Statement", Description = "Provider statements", IsActive = true }
        );

        // Seed Communication Type Statuses (mapping which statuses are valid for each type)
        var typeStatuses = new List<CommunicationTypeStatus>();

        // EOB valid statuses
        string[] eobStatuses = { "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "Inserted", "WarehouseReady", "Shipped", "InTransit", "Delivered", "Returned", "Failed", "Cancelled", "Archived" };
        for (int i = 0; i < eobStatuses.Length; i++)
        {
            typeStatuses.Add(new CommunicationTypeStatus { TypeCode = "EOB", StatusCode = eobStatuses[i], DisplayOrder = i + 1, Description = $"EOB {eobStatuses[i]} status" });
        }

        // EOP valid statuses
        string[] eopStatuses = { "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "Inserted", "WarehouseReady", "Shipped", "InTransit", "Delivered", "Returned", "Failed", "Cancelled", "Archived" };
        for (int i = 0; i < eopStatuses.Length; i++)
        {
            typeStatuses.Add(new CommunicationTypeStatus { TypeCode = "EOP", StatusCode = eopStatuses[i], DisplayOrder = i + 1, Description = $"EOP {eopStatuses[i]} status" });
        }

        // ID_CARD valid statuses
        string[] idCardStatuses = { "ReadyForRelease", "Released", "QueuedForPrinting", "Printed", "WarehouseReady", "Shipped", "InTransit", "Delivered", "Returned", "Failed", "Cancelled", "Expired", "Archived" };
        for (int i = 0; i < idCardStatuses.Length; i++)
        {
            typeStatuses.Add(new CommunicationTypeStatus { TypeCode = "ID_CARD", StatusCode = idCardStatuses[i], DisplayOrder = i + 1, Description = $"ID Card {idCardStatuses[i]} status" });
        }

        // Other types can have subset of statuses as needed
        string[] basicStatuses = { "ReadyForRelease", "Released", "Printed", "Shipped", "Delivered", "Failed", "Cancelled", "Archived" };
        
        foreach (var typeCode in new[] { "WELCOME_PACKET", "CLAIM_STATEMENT", "PROVIDER_STATEMENT" })
        {
            for (int i = 0; i < basicStatuses.Length; i++)
            {
                typeStatuses.Add(new CommunicationTypeStatus { TypeCode = typeCode, StatusCode = basicStatuses[i], DisplayOrder = i + 1, Description = $"{typeCode} {basicStatuses[i]} status" });
            }
        }

        modelBuilder.Entity<CommunicationTypeStatus>().HasData(typeStatuses);
    }
} 