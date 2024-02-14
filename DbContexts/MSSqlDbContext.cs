using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<EntityTrackerMSSql>().HasIndex(x => new { x.UniqueUserId, x.LocalId }).IsUnique();
        }
    }
}
