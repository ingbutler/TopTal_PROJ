using Microsoft.EntityFrameworkCore;
using DBRuns.Models;

namespace DBRuns.Data
{

    public class DBRunContext : DbContext
    {
        public DBRunContext(DbContextOptions<DBRunContext> options)
            : base(options)
        {
        }



        public DbSet<ItemsCount> ItemsCounts { get; set; }
        public DbSet<ReportItem> ReportItems { get; set; }
        public DbSet<Run> Runs { get; set; }
        public DbSet<User> Users { get; set; }




        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ItemsCount>().HasNoKey();
            modelBuilder.Entity<ReportItem>().HasNoKey();
        }

    }

}
