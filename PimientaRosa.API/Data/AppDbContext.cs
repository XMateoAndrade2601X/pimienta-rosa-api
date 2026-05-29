using Microsoft.EntityFrameworkCore;
using PimientaRosa.API.Models;

namespace PimientaRosa.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Usuario> Usuarios { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }
    public DbSet<MenuDia> MenusDia { get; set; }

    public DbSet<AdminUser> AdminUsers { get; set; }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Número de pedido único
        modelBuilder.Entity<Pedido>()
            .HasIndex(p => p.NumeroPedido)
            .IsUnique();

        // Precio con decimales correctos
        modelBuilder.Entity<Pedido>()
            .Property(p => p.Total)
            .HasColumnType("decimal(10,2)");
    }
}