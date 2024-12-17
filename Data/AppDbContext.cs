using bus_transport_mgt_sys.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace bus_transport_mgt_sys.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
    }

}
