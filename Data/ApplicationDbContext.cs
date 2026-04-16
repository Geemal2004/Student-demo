using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using BlindMatchPAS.Models;

namespace BlindMatchPAS.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ProjectProposal> Proposals => Set<ProjectProposal>();
    public DbSet<ResearchArea> ResearchAreas => Set<ResearchArea>();
    public DbSet<SupervisorMatch> SupervisorMatches => Set<SupervisorMatch>();
    public DbSet<SupervisorExpertise> SupervisorExpertises => Set<SupervisorExpertise>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Unique index on ResearchArea.Name
        builder.Entity<ResearchArea>()
            .HasIndex(r => r.Name)
            .IsUnique();

        // ProjectProposal with RowVersion concurrency token
        builder.Entity<ProjectProposal>(entity =>
        {
            entity.Property(p => p.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(p => p.Student)
                .WithMany(u => u.Proposals)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(p => p.ResearchArea)
                .WithMany(r => r.Proposals)
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.Property(p => p.Status)
                .HasConversion<string>();
        });

        // SupervisorMatch with RowVersion concurrency token
        builder.Entity<SupervisorMatch>(entity =>
        {
            entity.Property(m => m.RowVersion)
                .IsRowVersion()
                .IsConcurrencyToken();

            entity.HasOne(m => m.Proposal)
                .WithMany(p => p.SupervisorMatches)
                .HasForeignKey(m => m.ProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(m => m.Supervisor)
                .WithMany()
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasIndex(m => new { m.ProposalId, m.SupervisorId })
                .IsUnique();
        });

        // SupervisorExpertise join table
        builder.Entity<SupervisorExpertise>(entity =>
        {
            entity.HasOne(se => se.Supervisor)
                .WithMany(u => u.ExpertiseTags)
                .HasForeignKey(se => se.SupervisorId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(se => se.ResearchArea)
                .WithMany(r => r.SupervisorExpertises)
                .HasForeignKey(se => se.ResearchAreaId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasIndex(se => new { se.SupervisorId, se.ResearchAreaId })
                .IsUnique();
        });

        // AuditLog configuration
        builder.Entity<AuditLog>(entity =>
        {
            entity.HasKey(a => a.Id);
            entity.Property(a => a.Action).HasMaxLength(50).IsRequired();
            entity.Property(a => a.EntityType).HasMaxLength(100).IsRequired();
            entity.Property(a => a.EntityId).IsRequired();
            entity.Property(a => a.ActorUserId).HasMaxLength(450);
            entity.Property(a => a.OldValues).HasColumnType("nvarchar(max)");
            entity.Property(a => a.NewValues).HasColumnType("nvarchar(max)");
            entity.Property(a => a.IpAddress).HasMaxLength(50);
            entity.HasIndex(a => new { a.EntityType, a.EntityId });
            entity.HasIndex(a => a.Timestamp);
        });
    }
}
