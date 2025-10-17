using API.Data.Repositories;
using API.DTOs;
using API.Entities;
using API.Constants;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace API.Tests.Repositories;

public class VentaRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task CreateVenta_InsufficientStock_Returns_Error()
    {
        Context.Productos!.Add(new Producto { Id = 10, KioscoId = 1, Nombre = "Lapiz", Stock = 1, PrecioVenta = 5, PrecioCompra = 3, CategoriaId = 1, Sku = "0000000000010" });
        await Context.SaveChangesAsync();

        var repo = new VentaRepository(Context, Mapper);

        var dto = new VentaCreateDto { Productos = [ new ProductoVentaDto { ProductoId = 10, Cantidad = 2 } ] };
        var result = await repo.CreateVentaAsync(dto, kioscoId: 1, usuarioId: 1, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.InsufficientStock);
    }

    [Fact]
    public async Task CreateVenta_Success_Decrements_Stock()
    {
        Context.Productos!.Add(new Producto { Id = 20, KioscoId = 1, Nombre = "Lapiz", Stock = 3, PrecioVenta = 5, PrecioCompra = 3, CategoriaId = 1, Sku = "0000000000020" });
        await Context.SaveChangesAsync();

        var repo = new VentaRepository(Context, Mapper);

        var dto = new VentaCreateDto { Productos = [ new ProductoVentaDto { ProductoId = 20, Cantidad = 1 } ] };
        var result = await repo.CreateVentaAsync(dto, kioscoId: 1, usuarioId: 1, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        (await Context.Productos!.Where(p => p.Id == 20).Select(p => p.Stock).SingleAsync()).Should().Be(2);
    }
}
