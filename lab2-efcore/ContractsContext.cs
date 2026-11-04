using Microsoft.EntityFrameworkCore;
using ProjectContractsEf.Models;

namespace ProjectContractsEf;

public class ContractsContext : DbContext
{
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnConfiguring(DbContextOptionsBuilder options) =>
        options.UseSqlite($"Data Source={Path.Combine(AppContext.BaseDirectory, "contracts_ef.db")}");

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Position>(e =>
        {
            e.Property(p => p.Title).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Title).IsUnique();
            e.Property(p => p.Salary).HasConversion<double>();
            e.ToTable(t => t.HasCheckConstraint("CK_Salary", "Salary > 0"));
        });
        b.Entity<Employee>(e =>
        {
            e.Property(p => p.LastName).IsRequired().HasMaxLength(50);
            e.Property(p => p.FirstName).IsRequired().HasMaxLength(50);
            e.Property(p => p.MiddleName).IsRequired().HasMaxLength(50);
            e.Property(p => p.Address).IsRequired().HasMaxLength(200);
            e.HasOne(p => p.Position).WithMany(p => p.Employees)
             .HasForeignKey(p => p.PositionId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(p => p.FullName);
            e.Ignore(p => p.ShortName);
        });
        b.Entity<Project>(e =>
        {
            e.Property(p => p.Name).IsRequired().HasMaxLength(100);
            e.HasIndex(p => p.Name).IsUnique();
            e.ToTable(t => t.HasCheckConstraint("CK_Dates", "EndDate >= StartDate"));
        });
        b.Entity<Participation>(e =>
        {
            e.HasKey(p => new { p.EmployeeId, p.ProjectId });
            e.HasOne(p => p.Employee).WithMany(p => p.Participations).HasForeignKey(p => p.EmployeeId);
            e.HasOne(p => p.Project).WithMany(p => p.Participations).HasForeignKey(p => p.ProjectId);
            e.ToTable(t => t.HasCheckConstraint("CK_Days", "DaysWorked > 0"));
        });

        Seed.Apply(b);
    }
}
