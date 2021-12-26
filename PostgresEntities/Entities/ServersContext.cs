using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PostgresEntities.Entities
{
    public class ServersContext : DbContext
    {
        public DbSet<Server> Servers { get; set; }

        public ServersContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host={Environment.GetEnvironmentVariable("TOPDOWN_DATABASE_ADDRESS")};" +
                                     $"Port={Environment.GetEnvironmentVariable("TOPDOWN_DATABASE_PORT")};" +
                                     $"Database={Environment.GetEnvironmentVariable("TOPDOWN_DATABASE_NAME")};" +
                                     $"Username={Environment.GetEnvironmentVariable("TOPDOWN_DATABASE_USERNAME")};" +
                                     $"Password={Environment.GetEnvironmentVariable("TOPDOWN_DATABASE_PASSWORD")}"
            );
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            modelBuilder.Entity<Server>().HasKey(x => new {
                x.Address,
                x.Port
            });

            base.OnModelCreating(modelBuilder);
        }
    }
}
