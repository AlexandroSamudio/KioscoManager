using API.Data.Repositories;
using FluentAssertions;

namespace API.Tests.Repositories;

public class ReporteRepositoryTests : RepositoryTestBase
{
    private readonly ReporteRepository _repo;
    private readonly DateTime _now;

    public ReporteRepositoryTests()
    {
        _now = new DateTime(2025, 1, 15, 12, 0, 0, DateTimeKind.Utc);
        TestDataSeeder.SeedBasicCatalog(Context, _now);
        _repo = new ReporteRepository(Context);
    }

    [Fact]
    public async Task GetTopProductsByVentasAsync_PaginatesAndOrders()
    {
        var start = _now.AddDays(-3);
        var end = _now;
        var res = await _repo.GetTopProductsByVentasAsync(1, 1, 2, start, end, default);
        res.IsSuccess.Should().BeTrue();
        res.Data!.Should().HaveCount(2);
        res.Data.First().CantidadVendida.Should().BeGreaterThanOrEqualTo(res.Data.Last().CantidadVendida);
    }

    [Fact]
    public async Task CalculateKpiReporteAsync_ComputesKpis()
    {
        var start = _now.AddDays(-3);
        var end = _now;
        var res = await _repo.CalculateKpiReporteAsync(1, start, end, default);
        res.IsSuccess.Should().BeTrue();
        res.Data!.TotalVentas.Should().BeGreaterThan(0);
        res.Data.NumeroTransacciones.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetVentasPorDiaAsync_ReturnsData()
    {
        var start = _now.AddDays(-3);
        var end = _now;
        var res = await _repo.GetVentasPorDiaAsync(1, start, end, default);
        res.IsSuccess.Should().BeTrue();
        res.Data!.Count.Should().BeGreaterThan(0);
    }
}
