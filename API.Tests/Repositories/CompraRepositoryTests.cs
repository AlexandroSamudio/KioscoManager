using API.Data.Repositories;
using API.DTOs;
using API.Constants;
using FluentAssertions;

namespace API.Tests.Repositories;

public class CompraRepositoryTests : RepositoryTestBase
{
    [Fact]
    public async Task CreateCompraWithStockAdjustmentsAsync_ProductNotFound_Returns_EntityNotFound()
    {
        var repo = new CompraRepository(Context, Mapper);

        var compra = new CompraCreateDto
        {
            Detalles = new[] { new CompraDetalleDto { ProductoId = 10, Cantidad = 1, CostoUnitario = 5 } }
        };

        var result = await repo.CreateCompraWithStockAdjustmentsAsync(compra, kioscoId: 1, usuarioId: 1, CancellationToken.None);

        result.IsSuccess.Should().BeFalse();
        result.ErrorCode.Should().Be(ErrorCodes.EntityNotFound);
    }
}
