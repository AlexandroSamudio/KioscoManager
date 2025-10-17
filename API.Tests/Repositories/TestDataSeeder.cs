using API.Data;
using API.Entities;

namespace API.Tests.Repositories;

internal static class TestDataSeeder
{
    public static void SeedBasicCatalog(DataContext ctx, DateTime? utcNow = null)
    {
        if (ctx.Categorias!.Any()) return;

        var now = utcNow ?? DateTime.UtcNow;

        var catBebidas = new Categoria { Id = 1, Nombre = "Bebidas" };
        var catSnacks = new Categoria { Id = 2, Nombre = "Snacks" };
        var catLimpieza = new Categoria { Id = 3, Nombre = "Limpieza" };
        ctx.Categorias!.AddRange(catBebidas, catSnacks, catLimpieza);

        var productos = new List<Producto>
        {
            new() { Id=1, KioscoId=1, CategoriaId=1, Nombre="Agua 500ml", Sku="0000000000001", PrecioCompra=0.40m, PrecioVenta=0.80m, Stock=10 },
            new() { Id=2, KioscoId=1, CategoriaId=1, Nombre="Cola 330ml", Sku="0000000000002", PrecioCompra=0.50m, PrecioVenta=1.20m, Stock=2 },
            new() { Id=3, KioscoId=1, CategoriaId=2, Nombre="Papas", Sku="0000000000003", PrecioCompra=0.30m, PrecioVenta=0.90m, Stock=0 },
            new() { Id=4, KioscoId=2, CategoriaId=2, Nombre="Maní", Sku="0000000000004", PrecioCompra=0.20m, PrecioVenta=0.70m, Stock=5 },
            new() { Id=5, KioscoId=1, CategoriaId=3, Nombre="Lavandina", Sku="0000000000005", PrecioCompra=1.00m, PrecioVenta=1.70m, Stock=7 }
        };
        ctx.Productos!.AddRange(productos);

    // Ventas para reportes (kiosco 1)
    var venta1 = new Venta { Id = 1, KioscoId = 1, Fecha = now.AddDays(-2), Total = 4.0m, UsuarioId = 1 };
    var venta2 = new Venta { Id = 2, KioscoId = 1, Fecha = now.AddDays(-1), Total = 2.4m, UsuarioId = 1 };
        ctx.Ventas!.AddRange(venta1, venta2);

        ctx.DetalleVentas!.AddRange(
            new DetalleVenta { Id = 1, VentaId = 1, ProductoId = 1, Cantidad = 2, PrecioUnitario = 0.80m },
            new DetalleVenta { Id = 2, VentaId = 1, ProductoId = 2, Cantidad = 2, PrecioUnitario = 1.20m },
            new DetalleVenta { Id = 3, VentaId = 2, ProductoId = 3, Cantidad = 1, PrecioUnitario = 0.40m }
        );

    // Compras históricas para costos
    var compra = new Compra { Id = 1, KioscoId = 1, UsuarioId = 1, Fecha = now.AddDays(-10), CostoTotal = 100m, Detalles = new List<CompraDetalle>() };
        ctx.Compras!.Add(compra);
        ctx.CompraDetalles!.AddRange(
            new CompraDetalle { Id = 1, CompraId = 1, ProductoId = 1, Cantidad = 10, CostoUnitario = 0.35m },
            new CompraDetalle { Id = 2, CompraId = 1, ProductoId = 2, Cantidad = 10, CostoUnitario = 0.45m },
            new CompraDetalle { Id = 3, CompraId = 1, ProductoId = 3, Cantidad = 10, CostoUnitario = 0.20m }
        );

        ctx.SaveChanges();
    }
}
