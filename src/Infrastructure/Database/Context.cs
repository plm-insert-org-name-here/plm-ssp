using System.Linq;
using Domain.Common;
using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Infrastructure.Database;

public class Context : DbContext
{
    protected Context()
    {
    }

    public Context(DbContextOptions options) : base(options)
    {
    }

    public DbSet<Site> Sites => Set<Site>();
    public DbSet<OPU> OPUs => Set<OPU>();
    public DbSet<Line> Lines => Set<Line>();
    public DbSet<Station> Stations => Set<Station>();
    public DbSet<Location> Locations => Set<Location>();
    public DbSet<Detector> Detectors => Set<Detector>();
    public DbSet<Event> Events => Set<Event>();
    public DbSet<Job> Jobs => Set<Job>();
    public DbSet<Task> Tasks => Set<Task>();
    public DbSet<TaskInstance> TaskInstances => Set<TaskInstance>();
    public DbSet<Step> Steps => Set<Step>();
    public DbSet<Object> Objects => Set<Object>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Location>()
            .HasOne(l => l.Detector)
            .WithOne(d => d.Location)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
        
        modelBuilder.Entity<TaskInstance>().Property(x => x.RemainingStepIds).HasConversion(new ValueConverter<int[], string>(
            i => string.Join(",", i),
            s => string.IsNullOrWhiteSpace(s) ? new int[0] : s.Split(new[] { ',' }).Select(v => int.Parse(v)).ToArray()));
        
        modelBuilder.Entity<CalibrationCoordinates>().Property(x => x.Qr).HasConversion(new ValueConverter<int[], string>(
            i => string.Join(",", i),
            s => string.IsNullOrWhiteSpace(s) ? new int[0] : s.Split(new[] { ',' }).Select(v => int.Parse(v)).ToArray()));
        
        modelBuilder.Entity<CalibrationCoordinates>().Property(x => x.Tray).HasConversion(new ValueConverter<int[], string>(
            i => string.Join(",", i),
            s => string.IsNullOrWhiteSpace(s) ? new int[0] : s.Split(new[] { ',' }).Select(v => int.Parse(v)).ToArray()));
        
    }
}