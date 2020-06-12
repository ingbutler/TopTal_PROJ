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


        public DbSet<Run> Runs { get; set; }
        public DbSet<User> Users { get; set; }

    }

}
