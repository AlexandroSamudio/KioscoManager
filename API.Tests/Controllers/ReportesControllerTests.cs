using API.Entities;
using API.Controllers;
using API.DTOs;
using API.Helpers;
using API.Interfaces;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using API.Tests.TestInfrastructure;

namespace API.Tests.Controllers;

public class ReportesControllerTests : ControllerTestBase
{
    private readonly Mock<IReporteRepository> _repo = new();
    private readonly Mock<IVentaRepository> _ventaRepo = new();
    private readonly DateTime _now = new(2025, 2, 1, 0, 0, 0, DateTimeKind.Utc);

    private ReportesController CreateController(int kioscoId = 9)
    {
        var controller = new ReportesController(_repo.Object, _ventaRepo.Object);
        var principal = TestPrincipalFactory.Create(kioscoId: kioscoId);
        return SetupController(controller, principal);
    }

    [Fact]
    public async Task GetReporte_Success_Returns_200()
    {
        var dto = new ReporteDto { TotalVentas = 100, FechaInicio = _now.AddDays(-1), FechaFin = _now };
        _repo.Setup(r => r.CalculateKpiReporteAsync(9, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<ReporteDto>.Success(dto));

        var controller = CreateController(9);
        var res = await controller.GetReporte(new DateRangeDto { FechaInicio = _now.AddDays(-7), FechaFin = _now }, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(dto);
    }

    [Fact]
    public async Task GetTopProductos_Success_Returns_Paged_Ok_With_Header()
    {
        var paged = new PagedList<ProductoMasVendidoDto>(new[] { new ProductoMasVendidoDto { ProductoId = 1 } }, 1, 1, 10);
        _repo.Setup(r => r.GetTopProductsByVentasAsync(9, 1, 10, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<PagedList<ProductoMasVendidoDto>>.Success(paged));

        var controller = CreateController(9);
        var res = await controller.GetTopProductos(CancellationToken.None, new DateRangeDto { FechaInicio = _now.AddDays(-30), FechaFin = _now }, 1, 10);

        var ok = res.Result as OkObjectResult;
        ok.Should().NotBeNull();
        ok!.Value.Should().BeEquivalentTo(paged);
        controller.Response.Headers.ContainsKey("Pagination").Should().BeTrue();
    }

    [Fact]
    public async Task GetVentasPorDia_Success_Returns_200()
    {
        var list = new List<VentasPorDiaDto> { new() { Fecha = _now, TotalVentas = 1 } } as IReadOnlyList<VentasPorDiaDto>;
        _repo.Setup(r => r.GetVentasPorDiaAsync(9, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<IReadOnlyList<VentasPorDiaDto>>.Success(list));

        var controller = CreateController(9);
        var res = await controller.GetVentasPorDia(new DateRangeDto { FechaInicio = _now.AddDays(-7), FechaFin = _now }, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetCategoriasRentabilidad_Success_Returns_200()
    {
        var list = new List<CategoriasRentabilidadDto> { new() { CategoriaId = 1, PorcentajeVentas = 0.5m } } as IReadOnlyList<CategoriasRentabilidadDto>;
        _repo.Setup(r => r.GetCategoriasRentabilidadAsync(9, It.IsAny<DateTime>(), It.IsAny<DateTime>(), It.IsAny<CancellationToken>()))
             .ReturnsAsync(Result<IReadOnlyList<CategoriasRentabilidadDto>>.Success(list));

        var controller = CreateController(9);
        var res = await controller.GetCategoriasRentabilidad(new DateRangeDto { FechaInicio = DateTime.UtcNow.AddDays(-7), FechaFin = DateTime.UtcNow }, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }

    [Fact]
    public async Task GetVentasParaChart_Success_Returns_200()
    {
        var list = new List<VentaChartDto> { new() { Id = 1, Fecha = DateTime.UtcNow, Total = 10 } } as IReadOnlyList<VentaChartDto>;
        _ventaRepo.Setup(r => r.GetVentasIndividualesDelDiaAsync(9, It.IsAny<CancellationToken>(), It.IsAny<DateTime?>()))
                  .ReturnsAsync(Result<IReadOnlyList<VentaChartDto>>.Success(list));

        var controller = CreateController(9);
        var res = await controller.GetVentasParaChart(null, CancellationToken.None);

        res.Result.Should().BeNull();
        res.Value.Should().BeEquivalentTo(list);
    }
}
