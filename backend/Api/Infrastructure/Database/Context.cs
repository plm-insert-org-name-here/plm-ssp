using Api.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Api.Infrastructure.Database
{
    public class Context : DbContext
    {
        protected Context()
        {
        }

        public Context(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Detector> Detectors => Set<Detector>();
        public DbSet<Event> Events => Set<Event>();
        public DbSet<Job> Jobs => Set<Job>();
        public DbSet<Location> Locations => Set<Location>();
        public DbSet<Task> Tasks => Set<Task>();
        public DbSet<TaskResult> TaskResults => Set<TaskResult>();
        public DbSet<Template> Templates => Set<Template>();

    }
}