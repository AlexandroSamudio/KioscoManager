using API.Data.Repositories;
using API.Interfaces;
using FluentAssertions;

namespace API.Tests.Repositories;

public class CategoriaRepositoryTests : RepositoryTestBase
{
    private readonly ICategoriaRepository _repo;

    public CategoriaRepositoryTests()
    {
        TestDataSeeder.SeedBasicCatalog(Context);
        _repo = new CategoriaRepository(Context, Mapper);
    }

    [Fact]
    public async Task GetCategoriasAsync_PaginatesAndOrdersByNombre()
    {
        var result = await _repo.GetCategoriasAsync(pageNumber:1, pageSize:2, default);
        result.IsSuccess.Should().BeTrue();
        result.Data!.Should().HaveCount(2);
        result.Data.Select(c => c.Nombre).Should().BeInAscendingOrder();
        result.Data.TotalCount.Should().Be(3);
        result.Data.TotalPages.Should().Be(2);
    }

    [Fact]
    public async Task GetCategoriaByIdAsync_ReturnsNotFound_WhenMissing()
    {
        var res = await _repo.GetCategoriaByIdAsync(999, default);
        res.IsSuccess.Should().BeFalse();
    }
}
