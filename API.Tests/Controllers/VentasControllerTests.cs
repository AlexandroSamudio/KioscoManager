using API.Entities;
using API.Controllers;
using API.DTOs;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class VentasControllerTests : ControllerTestBase
{
    private readonly Mock<IVentaRepository> _repo = new();
    private readonly DateTime _now = new(2025, 1, 10, 0, 0, 0, DateTimeKind.Utc);

    private VentasController CreateController(int kioscoId = 4, int userId = 21)
    {
        var controller = new VentasController(_repo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId, userId: userId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetVentasDelDia_Success_Returns_200()
    {
        var list = new List<VentaDto> { new() { Id = 1 } } as IReadOnlyList<VentaDto>;
        _repo.Setup(r => r.GetVentasDelDiaAsync(4, It.IsAny<CancellationToken>(), null))
             .ReturnsAsync(Result<IReadOnlyList<VentaDto>>.Success(list));

        var controller = CreateController(4, 21);
        var res = await controller.GetVentasDelDia(null, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetVentasRecientes_Success_Returns_200()
    {
        var list = new List<VentaDto> { new() { Id = 2 } } as IReadOnlyList<VentaDto>;
        _repo.Setup(r => r.GetVentasRecientesAsync(4, 4, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<IReadOnlyList<VentaDto>>.Success(list));

        var controller = CreateController(4, 21);
        var res = await controller.GetVentasRecientes(CancellationToken.None, 4);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetMontoTotalVentas_Success_Returns_200()
    {
        _repo.Setup(r => r.GetMontoTotalVentasDelDiaAsync(4, It.IsAny<CancellationToken>(), null))
             .ReturnsAsync(Result<decimal>.Success(123.45m));

        var controller = CreateController(4, 21);
        var res = await controller.GetMontoTotalVentas(null, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().Be(123.45m);
    }

    [Fact]
    public async Task GetProductosMasVendidosDelDia_Success_Returns_200()
    {
        var list = new List<ProductoMasVendidoDto> { new() { ProductoId = 1 } } as IReadOnlyList<ProductoMasVendidoDto>;
        _repo.Setup(r => r.GetProductosMasVendidosDelDiaAsync(4, 4, It.IsAny<CancellationToken>(), null))
             .ReturnsAsync(Result<IReadOnlyList<ProductoMasVendidoDto>>.Success(list));

        var controller = CreateController(4, 21);
        var res = await controller.GetProductosMasVendidosDelDia(null, CancellationToken.None, 4);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task FinalizarVenta_Success_Returns_201()
    {
        var create = new VentaCreateDto { Productos = new() { new ProductoVentaDto { ProductoId = 1, Cantidad = 1 } } };
        var created = new VentaDto { Id = 99, Fecha = _now, Total = 10 };
        _repo.Setup(r => r.CreateVentaAsync(create, 4, 21, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<VentaDto>.Success(created));

        var controller = CreateController(4, 21);
        var res = await controller.FinalizarVenta(create, CancellationToken.None);

        var createdAt = res.Result as CreatedAtActionResult;
        createdAt!.StatusCode.Should().Be(201);
        createdAt.Value.Should().BeEquivalentTo(created);
    }

    [Fact]
    public void GetVentasForExport_BadRange_Returns_400()
    {
        var controller = CreateController();
        var res = controller.GetVentasForExport(CancellationToken.None, _now, _now.AddDays(-1), null);

        var obj = res.Result as ObjectResult;
        obj!.StatusCode.Should().Be(400);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public void GetVentasForExport_Success_Returns_200()
    {
        var data = AsyncEnumerable.Empty<VentaDto>();
        _repo.Setup(r => r.GetVentasForExport(4, It.IsAny<CancellationToken>(), null, null, null))
             .Returns(Result<IAsyncEnumerable<VentaDto>>.Success(data));

        var controller = CreateController(4, 21);
        var res = controller.GetVentasForExport(CancellationToken.None, null, null, null);

        res.Result.Should().BeNull();
        res.Value.Should().BeSameAs(data);
    }
}
