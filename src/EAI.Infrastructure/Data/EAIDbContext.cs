using EAI.Core.Interfaces;
using EAI.Core.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace EAI.Infrastructure.Data;

public class EAIDbContext : DbContext
{
    public EAIDbContext(DbContextOptions<EAIDbContext> options) : base(options)
    {
    }

    public DbSet<PolicyDocument> Policies { get; set; }
    public DbSet<AuditLog> AuditLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<PolicyDocument>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.PolicyName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PolicyType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Content).IsRequired();
            entity.Property(e => e.Version).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Department).HasMaxLength(100);
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Decision).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Reasoning).IsRequired();
            entity.Property(e => e.UserId).IsRequired().HasMaxLength(50);
            entity.Property(e => e.IpAddress).HasMaxLength(45);
            entity.Property(e => e.UserAgent).HasMaxLength(500);
        });
    }
}
