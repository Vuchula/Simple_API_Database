using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using static Simple_API_Database.Models.EF_Models;

namespace Simple_API_Database.DataAccess
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Company> Companies { get; set; }
        public DbSet<News> News_all { get; set; }
        public DbSet<Sector> Sector_all { get; set; }
        public DbSet<Loser> Loser_all { get; set; }
        public DbSet<Gainer> Gainer_all { get; set; }
    }
}
