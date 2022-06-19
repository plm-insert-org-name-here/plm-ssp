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

        public DbSet<Location> Locations { get; set; } = default!;
        public DbSet<Detector> Detectors { get; set; } = default!;
    }
}