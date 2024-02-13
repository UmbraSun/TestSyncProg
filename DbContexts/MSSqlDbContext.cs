using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using TestSyncProg.Entity;

namespace TestSyncProg.DbContexts
{
    public class MSSqlDbContext : DbContext
    {
        public DbSet<MaterialMSSql> Materials { get; set; }

        public DbSet<EntityTrackerMSSql> EntityTracker { get; set; }

        public MSSqlDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlServer("server =.;database=TestDb;trusted_connection = true");
        }
    }
}
