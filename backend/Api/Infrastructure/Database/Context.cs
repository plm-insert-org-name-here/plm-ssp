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

        public DbSet<Detector> Detectors { get; set; } = default!;
        public DbSet<Event> Events { get; set; } = default!;
        public DbSet<Job> Jobs { get; set; } = default!;
        public DbSet<Location> Locations { get; set; } = default!;
        public DbSet<Task> Tasks { get; set; } = default!;
        public DbSet<TaskResult> TaskResults { get; set; } = default!;
        public DbSet<Template> Templates { get; set; } = default!;

    }
}