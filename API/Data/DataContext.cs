using API.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace API.Data
{
    public class DataContext : IdentityDbContext<AppUser, AppRole, int,
                                                IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
                                                IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public DataContext(DbContextOptions<DataContext> options) : base(options)
        {
        }

        public DbSet<Producto>? Productos { get; set; }
        public DbSet<Categoria>? Categorias { get; set; }
        public DbSet<Venta>? Ventas { get; set; }
        public DbSet<DetalleVenta>? DetalleVentas { get; set; }
        public DbSet<Kiosco> Kioscos { get; set; }
        public DbSet<CodigoInvitacion> CodigosInvitacion { get; set; }
        public DbSet<Compra>? Compras { get; set; }
        public DbSet<CompraDetalle>? CompraDetalles { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUserRole>(userRole =>
            {
                userRole.HasKey(ur => new { ur.UserId, ur.RoleId });

                userRole.HasOne(ur => ur.Role)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.RoleId)
                    .IsRequired();

                userRole.HasOne(ur => ur.User)
                    .WithMany(r => r.UserRoles)
                    .HasForeignKey(ur => ur.UserId)
                    .IsRequired();
            });

            builder.Entity<Kiosco>()
                .HasMany(k => k.Productos)
                .WithOne(p => p.Kiosco)
                .HasForeignKey(p => p.KioscoId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Entity<Kiosco>()
                .HasMany(k => k.Usuarios)
                .WithOne(u => u.Kiosco)
                .HasForeignKey(u => u.KioscoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Kiosco>()
                .HasMany(k => k.Ventas)
                .WithOne(v => v.Kiosco)
                .HasForeignKey(v => v.KioscoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Kiosco>()
                .HasMany(k => k.CodigosInvitacion)
                .WithOne(ic => ic.Kiosco)
                .HasForeignKey(ic => ic.KioscoId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<CodigoInvitacion>()
                .HasIndex(ic => ic.Code)
                .IsUnique();

            builder.Entity<Producto>()
                .HasIndex(p => new { p.KioscoId, p.Sku })
                .IsUnique();

            builder.Entity<Kiosco>()
                .HasMany(k => k.Compras)
                .WithOne(c => c.Kiosco)
                .HasForeignKey(c => c.KioscoId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<AppUser>()
                .HasMany<Compra>()
                .WithOne(c => c.Usuario)
                .HasForeignKey(c => c.UsuarioId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Compra>()
                .HasMany(c => c.Detalles)
                .WithOne(cd => cd.Compra)
                .HasForeignKey(cd => cd.CompraId)
                .OnDelete(DeleteBehavior.Cascade);
        }

    }
}
