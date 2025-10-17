using API.Data.Repositories;
using API.DTOs;
using API.Enums;
using API.Interfaces;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Moq;

namespace API.Tests.Repositories;

public class ProductoRepositoryTests : RepositoryTestBase
{
    private readonly IProductoRepository _repo;

    public ProductoRepositoryTests()
    {
        TestDataSeeder.SeedBasicCatalog(Context);
        var photo = new Mock<IPhotoService>();
        var logger = new Mock<ILogger<ProductoRepository>>();
        _repo = new ProductoRepository(Context, Mapper, photo.Object, logger.Object);
    }

    [Fact]
    public async Task GetProductosPaginatedAsync_FiltersByCategoria_AndSearch_AndStock()
    {
        var res = await _repo.GetProductosPaginatedAsync(default, kioscoId:1, pageNumber:1, pageSize:10,
            categoriaId:1, stockStatus: StockStatus.Low, searchTerm:"cola", sortColumn:"nombre", sortDirection:"asc");
        res.IsSuccess.Should().BeTrue();
        res.Data!.Should().ContainSingle();
        res.Data.First().Nombre.Should().Contain("Cola");
    }

    [Fact]
    public async Task GetProductoBySkuAsync_ReturnsNotFound_WhenMissing()
    {
        var res = await _repo.GetProductoBySkuAsync(1, "9999999999999", default);
        res.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task CreateProductoAsync_RejectsDuplicateSkuPerKiosco()
    {
        var dto = new ProductoCreateDto { Nombre = "Otro", CategoriaId = 1, PrecioCompra = 1, PrecioVenta = 2, Stock = 1, Sku = "0000000000001" };
        var res = await _repo.CreateProductoAsync(1, dto, default);
        res.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public async Task GetProductosByLowestStockAsync_ReturnsOrderedByStockAscending()
    {
        var res = await _repo.GetProductosByLowestStockAsync(3, kioscoId:1, default);
        res.IsSuccess.Should().BeTrue();
        res.Data!.Select(p => p.Stock).Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task GetCapitalInvertidoTotalAsync_SumsCorrectly()
    {
        var res = await _repo.GetCapitalInvertidoTotalAsync(1, default);
        res.IsSuccess.Should().BeTrue();
        res.Data.Should().Be(0.40m*10 + 0.50m*2 + 0.30m*0 + 1.00m*7);
    }
}
