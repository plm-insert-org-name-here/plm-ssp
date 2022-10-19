using Domain.Entities;
using Domain.Entities.CompanyHierarchy;
using Domain.Entities.TaskAggregate;
using Microsoft.EntityFrameworkCore;

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
    public DbSet<Template> Templates => Set<Template>();
    public DbSet<Step> StateChanges => Set<Step>();

}