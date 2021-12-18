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

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseNpgsql($"Host={System.Configuration.ConfigurationManager.AppSettings.Get("DbAddress")};" +
                                     $"Port={System.Configuration.ConfigurationManager.AppSettings.Get("DbPort")};" +
                                     $"Database={System.Configuration.ConfigurationManager.AppSettings.Get("DbName")};" +
                                     $"Username={System.Configuration.ConfigurationManager.AppSettings.Get("DbUserName")};" +
                                     $"Password={System.Configuration.ConfigurationManager.AppSettings.Get("DbUserPassword")}"
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
