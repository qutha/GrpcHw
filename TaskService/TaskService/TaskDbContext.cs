using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService;

public sealed class TaskDbContext : DbContext
{
    public DbSet<TaskEntity> Tasks { get; set; }

    public TaskDbContext(DbContextOptions<TaskDbContext> options)
        : base(options) => Database.EnsureCreated();

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<TaskEntity>(entity =>
        {
            entity.HasKey(t => t.Id);
            entity
                .Property(t => t.Tags)
                .HasConversion(
                    v => string.Join(",", v),
                    v => v.Split(",", StringSplitOptions.RemoveEmptyEntries).ToList()
                );
        });
}
