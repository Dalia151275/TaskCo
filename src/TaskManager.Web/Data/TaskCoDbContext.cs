using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Models.Entities;

namespace TaskManager.Web.Data;

public class TaskCoDbContext : IdentityDbContext<User>
{
    public TaskCoDbContext(DbContextOptions<TaskCoDbContext> options) : base(options) { }

    public new DbSet<User> Users => Set<User>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<TaskItem> TaskItems => Set<TaskItem>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(200).IsRequired();
            e.Property(p => p.Description).HasMaxLength(2000);
            e.HasOne(p => p.Owner)
             .WithMany()
             .HasForeignKey(p => p.OwnerId)
             .OnDelete(DeleteBehavior.Cascade);
            e.HasMany(p => p.Tasks)
             .WithOne(t => t.Project)
             .HasForeignKey(t => t.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).HasMaxLength(500).IsRequired();
            e.Property(t => t.Description).HasMaxLength(5000);
            // NoAction avoids a multiple cascade path: User→Project→TaskItem already covers cleanup.
            e.HasOne(t => t.Owner)
             .WithMany()
             .HasForeignKey(t => t.OwnerId)
             .OnDelete(DeleteBehavior.NoAction);
        });
    }
}
