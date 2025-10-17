using API.Entities;
using API.Controllers;
using API.DTOs;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class ComprasControllerTests : ControllerTestBase
{
    private readonly Mock<ICompraRepository> _repo = new();

    private ComprasController CreateController(int kioscoId = 7, int userId = 42)
    {
        var controller = new ComprasController(_repo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId, userId: userId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetCompra_Success_Returns_200_With_Compra()
    {
        var dto = new CompraDto { Id = 10, Fecha = DateTime.UtcNow, CostoTotal = 100, UsuarioId = 42, Detalles = new List<CompraDetalleViewDto>() };
        _repo.Setup(r => r.GetCompraByIdAsync(7, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<CompraDto>.Success(dto));

        var controller = CreateController(7, 42);
        var res = await controller.GetCompra(10, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task CreateCompra_Success_Returns_201()
    {
        var create = new CompraCreateDto { Proveedor = "Prov" };
        var created = new CompraDto { Id = 5, Fecha = DateTime.UtcNow, CostoTotal = 50, UsuarioId = 42, Detalles = new List<CompraDetalleViewDto>() };
        _repo.Setup(r => r.CreateCompraWithStockAdjustmentsAsync(create, 7, 42, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<CompraDto>.Success(created));

        var controller = CreateController(7, 42);
        var res = await controller.CreateCompra(create, CancellationToken.None);

        var createdAt = res.Result as CreatedAtActionResult;
        createdAt!.StatusCode.Should().Be(201);
        createdAt.Value.Should().BeEquivalentTo(created);
        createdAt.RouteValues!["id"].Should().Be(5);
    }

    [Fact]
    public void GetComprasForExport_BadRange_Returns_400()
    {
        var controller = CreateController();
        var res = controller.GetComprasForExport(CancellationToken.None, DateTime.UtcNow, DateTime.UtcNow.AddDays(-1), null);

        var obj = res.Result as ObjectResult;
        obj!.StatusCode.Should().Be(400);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public void GetComprasForExport_Success_Returns_200()
    {
        var data = AsyncEnumerable.Empty<CompraDto>();
        _repo.Setup(r => r.GetComprasForExport(7, It.IsAny<CancellationToken>(), null, null, null))
             .Returns(Result<IAsyncEnumerable<CompraDto>>.Success(data));

        var controller = CreateController(7, 42);
        var res = controller.GetComprasForExport(CancellationToken.None, null, null, null);

        res.Result.Should().BeNull();
        res.Value.Should().BeSameAs(data);
    }
}
