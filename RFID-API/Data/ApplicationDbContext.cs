using Microsoft.EntityFrameworkCore;
using RFID_API.Models;

namespace RFID_API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<RFIDAccesos> RFID_ACCESOS { get; set; }
        public DbSet<RFIDRegistros> RFID_REGISTRO { get; set; }
        public DbSet<RFIDTarjetas> RFID_TARJETAS { get; set; }
        public DbSet<RFIDUsuarios> RFID_USUARIOS{ get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // RFID_ACCESOS: PK = IdAccesos (string)
            modelBuilder.Entity<RFIDAccesos>()
                .HasKey(a => a.IdAccesos);

            // RFID_REGISTRO: PK = IdRegistro, FK = IdAccesos
            modelBuilder.Entity<RFIDRegistros>()
                .HasKey(r => r.IdRegistro);

            modelBuilder.Entity<RFIDRegistros>()
                .HasOne(r => r.Acceso)
                .WithMany(a => a.Registros)
                .HasForeignKey(r => r.IdAccesos)
                .HasPrincipalKey(a => a.IdAccesos);

            modelBuilder.Entity<RFIDTarjetas>()
                .HasKey(t => t.IdTarjeta);

            modelBuilder.Entity<RFIDUsuarios>()
                .ToTable("RFID_USUARIOS")
                .HasKey(u => u.IdUsuario);
        }
    }
}
