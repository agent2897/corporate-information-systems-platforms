using Microsoft.EntityFrameworkCore;
using ProjectContractsBlazor.Models;

namespace ProjectContractsBlazor.Data;

public class ContractsContext(DbContextOptions<ContractsContext> options) : DbContext(options)
{
    public DbSet<Position> Positions => Set<Position>();
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<Participation> Participations => Set<Participation>();

    protected override void OnModelCreating(ModelBuilder b)
    {
        b.Entity<Position>(e =>
        {
            e.HasIndex(p => p.Title).IsUnique();
            e.Property(p => p.Salary).HasConversion<double>();
            e.ToTable(t => t.HasCheckConstraint("CK_Salary", "Salary > 0"));
        });
        b.Entity<Employee>(e =>
        {
            e.HasOne(p => p.Position).WithMany(p => p.Employees)
             .HasForeignKey(p => p.PositionId).OnDelete(DeleteBehavior.Restrict);
            e.Ignore(p => p.FullName);
        });
        b.Entity<Project>(e =>
        {
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
