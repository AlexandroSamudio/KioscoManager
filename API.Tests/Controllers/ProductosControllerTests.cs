using API.Controllers;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using API.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class ProductosControllerTests : ControllerTestBase
{
    private readonly Mock<IProductoRepository> _repo = new();

    private ProductosController CreateController(int kioscoId = 5)
    {
        var controller = new ProductosController(_repo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetProducto_Success_Returns_200_With_Producto()
    {
        var producto = new ProductoDto { Id = 7, Nombre = "Lapiz" };
        _repo.Setup(r => r.GetProductoByIdAsync(5, 7, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductoDto>.Success(producto));

        var controller = CreateController(5);
        var result = await controller.GetProducto(7, CancellationToken.None);

        result.Result.Should().BeNull();
        result.Value.Should().BeEquivalentTo(producto);
        _repo.Verify(r => r.GetProductoByIdAsync(5, 7, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateProducto_Success_Returns_201_CreatedAt()
    {
        var dto = new ProductoCreateDto { Nombre = "Cuaderno", CategoriaId = 1, PrecioCompra = 10, PrecioVenta = 15, Stock = 5 };
        var created = new ProductoDto { Id = 11, Nombre = dto.Nombre };
        _repo.Setup(r => r.CreateProductoAsync(5, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductoDto>.Success(created));

        var controller = CreateController(5);
        var result = await controller.CreateProducto(dto, CancellationToken.None);

        var createdAt = result.Result as CreatedAtActionResult;
        createdAt.Should().NotBeNull();
        createdAt!.StatusCode.Should().Be(201);
        createdAt.Value.Should().BeEquivalentTo(created);
        createdAt.RouteValues!["id"].Should().Be(11);
        _repo.Verify(r => r.CreateProductoAsync(5, dto, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProducto_Success_Returns_204_NoContent()
    {
        var update = new ProductoUpdateDto { Nombre = "Nuevo" };
        _repo.Setup(r => r.UpdateProductoAsync(5, 9, update, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = CreateController(5);
        var result = await controller.UpdateProducto(9, update, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _repo.Verify(r => r.UpdateProductoAsync(5, 9, update, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProducto_Success_Returns_204_NoContent()
    {
        _repo.Setup(r => r.DeleteProductoAsync(5, 12, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result.Success());

        var controller = CreateController(5);
        var result = await controller.DeleteProducto(12, CancellationToken.None);

        result.Should().BeOfType<NoContentResult>();
        _repo.Verify(r => r.DeleteProductoAsync(5, 12, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetProducto_NotFound_Returns_404()
    {
        _repo.Setup(r => r.GetProductoByIdAsync(5, 99, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductoDto>.Failure(ErrorCodes.EntityNotFound, "no existe"));

        var controller = CreateController(5);
        var result = await controller.GetProducto(99, CancellationToken.None);

        var obj = result.Result as ObjectResult;
        obj.Should().NotBeNull();
        obj!.StatusCode.Should().Be(404);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task CreateProducto_ValidationError_Returns_400()
    {
        var dto = new ProductoCreateDto { Nombre = null!, CategoriaId = 0 };
        _repo.Setup(r => r.CreateProductoAsync(5, dto, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Result<ProductoDto>.Failure(ErrorCodes.ValidationError, "invalid"));

        var controller = CreateController(5);
        var result = await controller.CreateProducto(dto, CancellationToken.None);

        var obj = result.Result as ObjectResult;
        obj.Should().NotBeNull();
        obj!.StatusCode.Should().Be(400);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }
}
