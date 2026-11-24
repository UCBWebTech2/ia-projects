using Microsoft.EntityFrameworkCore;
using PFEstudioJuridico.Models;
namespace PFEstudioJuridico.Data
{
    public class AppDBContext:DbContext
    {
        public AppDBContext(DbContextOptions<AppDBContext>options):base(options)
        {
            
        }
        public DbSet<Usuario> Usuarios { get; set; }
        //public DbSet<Auditoria> Auditorias { get; set; }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<Usuario>(tb=>
            {
                tb.HasKey(col => col.idUsuario);
                tb.Property(col => col.idUsuario)
                .UseIdentityColumn()
                .ValueGeneratedOnAdd();
                tb.Property(col => col.nombreUsuario).HasMaxLength(50);
                tb.Property(col => col.correoElectronico).HasMaxLength(50);
                tb.Property(col => col.contrasenia).HasMaxLength(50);

            });
            modelBuilder.Entity<Usuario>().ToTable("Usuarios");

            //modelBuilder.Entity<Auditoria>(tb =>  // Configura la entidad Auditoria
            //{
            //    tb.HasKey(col => col.IdAuditoria);
            //    tb.Property(col => col.IdUsuario);
            //    tb.Property(col => col.Accion).HasMaxLength(255);
            //    tb.Property(col => col.Descripcion).HasMaxLength(255);
            //    tb.Property(col => col.Fecha);
            //});

            //modelBuilder.Entity<Auditoria>().ToTable("Auditoria");

        }
    }
}
