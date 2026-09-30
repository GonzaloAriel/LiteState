using LiteState.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LiteState.Infrastructure
{
    public class LiteStateDbContext : IdentityDbContext<Usuario, IdentityRole<int>, int>
    {
        public LiteStateDbContext(DbContextOptions<LiteStateDbContext> options)
            : base(options)
        {
        }

        public DbSet<Empresa> Empresas => Set<Empresa>();
        public DbSet<Sector> Sectores => Set<Sector>();
        public DbSet<UsuarioSector> UsuarioSectores => Set<UsuarioSector>();
        public DbSet<Estado> Estados => Set<Estado>();
        public DbSet<Evento> Eventos => Set<Evento>();
        public DbSet<EstadoActual> EstadosActuales => Set<EstadoActual>();
        public DbSet<Dashboard> Dashboards => Set<Dashboard>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Empresa>()
                .HasMany(e => e.Usuarios)
                .WithOne(u => u.Empresa)
                .HasForeignKey(u => u.EmpresaId);

            modelBuilder.Entity<Sector>()
                .HasMany(s => s.UsuarioSectores)
                .WithOne(us => us.Sector)
                .HasForeignKey(us => us.SectorId);

            modelBuilder.Entity<Usuario>()
                .HasMany(u => u.UsuarioSectores)
                .WithOne(us => us.Usuario)
                .HasForeignKey(us => us.UsuarioId);

            modelBuilder.Entity<UsuarioSector>()
                .HasKey(us => new { us.UsuarioId, us.SectorId });

            modelBuilder.Entity<Estado>()
                .HasMany(e => e.Eventos)
                .WithOne(ev => ev.Estado)
                .HasForeignKey(ev => ev.EstadoId);

            modelBuilder.Entity<Estado>()
                .HasMany(e => e.EstadosActuales)
                .WithOne(ea => ea.Estado)
                .HasForeignKey(ea => ea.EstadoId);

            modelBuilder.Entity<EstadoActual>()
                .HasKey(ea => ea.SectorId);

            modelBuilder.Entity<EstadoActual>()
                .HasOne(ea => ea.Sector)
                .WithOne(s => s.EstadoActual)
                .HasForeignKey<EstadoActual>(ea => ea.SectorId);

            modelBuilder.Entity<EstadoActual>()
                .HasOne(ea => ea.Usuario)
                .WithMany()
                .HasForeignKey(ea => ea.UsuarioId);

            modelBuilder.Entity<Evento>()
                .HasOne(ev => ev.Empresa)
                .WithMany()
                .HasForeignKey(ev => ev.EmpresaId);

            modelBuilder.Entity<Evento>()
                .HasOne(ev => ev.Sector)
                .WithMany(s => s.Eventos)
                .HasForeignKey(ev => ev.SectorId);

            modelBuilder.Entity<Evento>()
                .HasOne(ev => ev.Usuario)
                .WithMany(u => u.Eventos)
                .HasForeignKey(ev => ev.UsuarioId);

            modelBuilder.Entity<Dashboard>()
                .HasOne(d => d.Empresa)
                .WithMany(e => e.Dashboards)
                .HasForeignKey(d => d.EmpresaId);

            modelBuilder.Entity<Dashboard>()
                .HasOne(d => d.Sector)
                .WithMany(s => s.Dashboards)
                .HasForeignKey(d => d.SectorId);

            modelBuilder.Entity<Evento>()
                .HasIndex(e => new { e.FechaHora, e.SectorId });

            modelBuilder.Entity<EstadoActual>()
                .HasIndex(ea => ea.EstadoId);

            modelBuilder.Entity<Dashboard>()
                .HasIndex(d => d.Token)
                .IsUnique();
        }
    }
}