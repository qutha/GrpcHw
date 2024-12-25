using Microsoft.EntityFrameworkCore;
using ProjectService.Models;

namespace ProjectService;

public sealed class ProjectDbContext : DbContext
{
    public DbSet<ProjectEntity> Projects { get; set; }

    public ProjectDbContext(DbContextOptions<ProjectDbContext> options)
        : base(options) => Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity
                .Property(p => p.ParticipantIds)
                .HasConversion(
                    v => string.Join(",", v),
                    v =>
                        v.Split(",", StringSplitOptions.RemoveEmptyEntries)
                            .Select(Guid.Parse)
                            .ToList()
                );
        });
}
