using API.Controllers;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Constants;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;
using API.Entities;

namespace API.Tests.Controllers;

public class CategoriasControllerTests : ControllerTestBase
{
    private readonly Mock<ICategoriaRepository> _repo = new();

    private CategoriasController CreateController()
    {
        var controller = new CategoriasController(_repo.Object);
        return SetupController(controller);
    }

    [Fact]
    public async Task GetCategorias_Success_Returns_Ok_With_PaginationHeader()
    {
        var items = new List<CategoriaDto> { new() { Id = 1, Nombre = "A" } };
        var paged = new PagedList<CategoriaDto>(items, count: 1, pageNumber: 1, pageSize: 10);
        _repo.Setup(r => r.GetCategoriasAsync(1, 10, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<PagedList<CategoriaDto>>.Success(paged));

        var controller = CreateController();
        var res = await controller.GetCategorias(CancellationToken.None, 1, 10);

        var ok = res.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(paged);
        controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetCategoria_NotFound_Returns_404()
    {
        _repo.Setup(r => r.GetCategoriaByIdAsync(9, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<CategoriaDto>.Failure(ErrorCodes.EntityNotFound, "no existe"));

        var controller = CreateController();
        var res = await controller.GetCategoria(9, CancellationToken.None);

        var obj = res.Result as ObjectResult;
        obj!.StatusCode.Should().Be(404);
        obj.Value.Should().BeOfType<ValidationProblemDetails>();
    }

    [Fact]
    public async Task CreateCategoria_Success_Returns_201()
    {
        var dto = new CategoriaCreateDto { Nombre = "Nueva" };
        var created = new CategoriaDto { Id = 3, Nombre = dto.Nombre };
        _repo.Setup(r => r.CreateCategoriaAsync(dto, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<CategoriaDto>.Success(created));

        var controller = CreateController();
        var res = await controller.CreateCategoria(dto, CancellationToken.None);

        var createdAt = res.Result as CreatedAtActionResult;
        createdAt!.StatusCode.Should().Be(201);
        createdAt.Value.Should().BeEquivalentTo(created);
        createdAt.RouteValues!["id"].Should().Be(3);
    }

    [Fact]
    public async Task UpdateCategoria_Success_Returns_204()
    {
        var upd = new CategoriaUpdateDto { Nombre = "Edit" };
        _repo.Setup(r => r.UpdateCategoriaAsync(2, upd, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result.Success());

        var controller = CreateController();
        var res = await controller.UpdateCategoria(2, upd, CancellationToken.None);

        res.Should().BeOfType<NoContentResult>();
    }

    [Fact]
    public async Task DeleteCategoria_Success_Returns_204()
    {
        _repo.Setup(r => r.DeleteCategoriaAsync(2, It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result.Success());

        var controller = CreateController();
        var res = await controller.DeleteCategoria(2, CancellationToken.None);

        res.Should().BeOfType<NoContentResult>();
    }
}
