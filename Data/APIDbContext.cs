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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Role>().ToTable("Roles");
            modelBuilder.Entity<Role>().HasKey(r => r.Id);
            modelBuilder.Entity<Role>().Property(r => r.Id).IsRequired().ValueGeneratedOnAdd();
            modelBuilder.Entity<Role>().Property(r => r.Name).IsRequired();
            modelBuilder.Entity<Role>().HasIndex(r => r.Name).IsUnique();

            modelBuilder.Entity<UserRole>().ToTable("UserRoles");
            modelBuilder.Entity<UserRole>().HasKey(ur => new { ur.UserId, ur.RoleId });

            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" },
                new Role { Id = 3, Name = "Manager" }
            );

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
           // optionsBuilder.UseLazyLoadingProxies();
        }

        public DbSet<User> Users { get; set; }
        public DbSet<Geolocation> Geolocation { get; set; }
        public DbSet<Point> Point { get; set; }
        public DbSet<Supplier> Supplier { get; set; }
        public DbSet<Technician> Technician { get; set; }
        public DbSet<Project> Project { get; set; }
        public DbSet<BotError> BotError { get; set; }
        public DbSet<ProductDomain> Product_Domain { get; set; }
        public DbSet<SupplierDomain> Supplier_Domain { get; set; }
        public DbSet<ProjectProduct> Project_Product { get; set; }
        public DbSet<ProjectGeolocation> Project_Geolocation { get; set; }
        public DbSet<ProjectPoint> Project_Point { get; set; }
        public DbSet<ProjectSupplier> Project_Supplier { get; set; }
        public DbSet<ProjectTechnician> Project_Technician { get; set; }
        public DbSet<ProjectElectric> Project_Electric { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<ProjectFile> Project_File { get; set; }
        public DbSet<FileModel> FileModel { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Mandala> Mandala { get; set; }
        public DbSet<UserRole> UserRoles { get; set; }
        public DbSet<UserToken> User_Token { get; set; }
        public DbSet<SalesReport> SalesReport { get; set; }
    }
}
