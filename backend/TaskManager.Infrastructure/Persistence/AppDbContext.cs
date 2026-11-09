using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using TaskManager.Domain.Entities;

namespace TaskManager.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<TaskItem> Tasks => Set<TaskItem>();

    // Lets LINQ queries call SQL Server's JSON_VALUE; it only translates to SQL and is never executed in C#.
    public static string? JsonValue(string? expression, string path) =>
        throw new NotSupportedException("JsonValue can only be used inside a LINQ query.");

    // The database stores UTC but EF returns dates of unspecified kind, which the API would serialize without a "Z"
    // and browsers would then read as local time.
    protected override void ConfigureConventions(ModelConfigurationBuilder builder)
    {
        builder.Properties<DateTime>().HaveConversion<UtcDateTimeConverter>();
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDbFunction(() => JsonValue(default, default!)).HasName("JSON_VALUE").IsBuiltIn();

        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");
            entity.Property(u => u.Name).HasMaxLength(100).IsRequired();
            entity.Property(u => u.Email).HasMaxLength(150).IsRequired();
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
        });

        modelBuilder.Entity<TaskItem>(entity =>
        {
            // The table is named Tasks while the class is TaskItem (avoids clashing with System.Threading.Tasks.Task).
            entity.ToTable("Tasks");
            entity.Property(t => t.Title).HasMaxLength(200).IsRequired();
            entity.Property(t => t.Description).HasMaxLength(1000);
            // Stored as text ('Pending', 'InProgress', 'Done') to match the CHECK constraint.
            entity.Property(t => t.Status).HasConversion<string>().HasMaxLength(20);
            entity.Property(t => t.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.Property(t => t.UpdatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            entity.HasOne(t => t.User).WithMany(u => u.Tasks).HasForeignKey(t => t.UserId);
        });
    }
}

public class UtcDateTimeConverter()
    : ValueConverter<DateTime, DateTime>(
        v => v,
        v => DateTime.SpecifyKind(v, DateTimeKind.Utc));
