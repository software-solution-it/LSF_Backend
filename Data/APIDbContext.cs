using LSF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LSF.Data
{
    public class APIDbContext : DbContext
    {
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLazyLoadingProxies();
        }

        public DbSet<User> AspNetUsers { get; set; }
        public DbSet<Geolocation> Geolocation { get; set; }
        public DbSet<Point> Point { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Technician> Technician { get; set; }
        public DbSet<UserGeolocation> User_Geolocation { get; set; }
        public DbSet<UserPoint> User_Point { get; set; }
        public DbSet<UserSupplier> User_Supplier { get; set; }
        public DbSet<UserTechnician> User_Technician { get; set; }
    }
}
