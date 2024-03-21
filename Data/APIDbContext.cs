using LSF.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;

namespace LSF.Data
{
    public class APIDbContext : IdentityDbContext
    {
        public APIDbContext(DbContextOptions<APIDbContext> options) : base(options)
        {

        }

        public DbSet<User> User { get; set; }
        public DbSet<Tecnicos> Tecnicos { get; set; }
        public DbSet<Produtos> Produtos { get; set; }
        public DbSet<Geolocalizacao> Geolocalizacao { get; set; }
        public DbSet<Lavanderia> Lavanderia { get; set; }
        public DbSet<Empresa> Empresa { get; set; }
        public DbSet<Maquina> Maquina { get; set; }
        public DbSet<MaquinaLavar> MaquinaLavar { get; set; }
        public DbSet<Marca> Marca { get; set; }
        public DbSet<MarcaEmpresa> MarcaEmpresa { get; set; }
        public DbSet<Municipio> Municipio { get; set; }
    }
}
