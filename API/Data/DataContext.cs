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
        }
    }
}
