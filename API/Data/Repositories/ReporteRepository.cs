using API.DTOs;
using API.Helpers;
using API.Interfaces;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public enum GroupingType
    {
        Daily,    
        Weekly,   
        Monthly
    }

    public class ReporteRepository(DataContext context) : IReporteRepository
    {
        public async Task<Result<ReporteDto>> CalculateKpiReporteAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            var ventasQuery = context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha <= fechaFin);

            var numeroTransacciones = await ventasQuery.CountAsync(cancellationToken);

            var totalVentas = await ventasQuery
                .SumAsync(v => v.Total, cancellationToken);

            var detallesVenta = await context.DetalleVentas!
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId && d.Venta.Fecha >= fechaInicio && d.Venta.Fecha <= fechaFin)
                .ToListAsync(cancellationToken);

            var productosYFechas = detallesVenta
                .Select(d => (d.ProductoId, d.Venta!.Fecha))
                .Distinct()
                .ToList();

            var costosHistoricos = await ObtenerCostosHistoricosAsync(productosYFechas, cancellationToken);

            decimal costoMercaderia = 0;

            foreach (var detalle in detallesVenta)
            {
                var costoHistorico = costosHistoricos.TryGetValue(detalle.ProductoId, out var costo) ? costo : null;
                decimal costoPorUnidad = costoHistorico ?? detalle.Producto!.PrecioCompra;
                costoMercaderia += detalle.Cantidad * costoPorUnidad;
            }

            decimal gananciaBruta = totalVentas - costoMercaderia;

            var reporte = new ReporteDto
            {
                TotalVentas = totalVentas,
                CostoMercaderiaVendida = costoMercaderia,
                GananciaBruta = gananciaBruta,
                NumeroTransacciones = numeroTransacciones,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            return Result<ReporteDto>.Success(reporte);
        }

        public async Task<Result<PagedList<ProductoMasVendidoDto>>> GetTopProductsByVentasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            var query = context.DetalleVentas!
                .Include(d => d.Producto)
                .Include(d => d.Producto!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                           d.Venta.Fecha >= fechaInicio &&
                           d.Venta.Fecha <= fechaFin)
                .GroupBy(d => new
                {
                    d.ProductoId,
                    d.Producto!.Nombre,
                    d.Producto.Sku,
                    Categoria = d.Producto.Categoria!.Nombre
                })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    Sku = g.Key.Sku,
                    CategoriaNombre = g.Key.Categoria,
                    CantidadVendida = g.Sum(d => d.Cantidad),
                    TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario)
                })
                .OrderByDescending(p => p.CantidadVendida);

            var result = await PagedList<ProductoMasVendidoDto>.CreateAsync(
                query.AsNoTracking(), pageNumber, pageSize, cancellationToken);

            return Result<PagedList<ProductoMasVendidoDto>>.Success(result);
        }

        public async Task<Result<IReadOnlyList<VentasPorDiaDto>>> GetVentasPorDiaAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            var rangeDays = (fechaFin.Date - fechaInicio.Date).Days + 1;
            var groupingType = DetermineGroupingType(rangeDays);
            var tipoAgrupacionString = GetGroupingTypeString(groupingType);

            IQueryable<VentasPorDiaDto> query = groupingType switch
            {
                GroupingType.Daily => context.DetalleVentas!
                    .Include(d => d.Venta)
                    .Where(d => d.Venta!.KioscoId == kioscoId &&
                                d.Venta.Fecha >= fechaInicio &&
                                d.Venta.Fecha <= fechaFin)
                    .GroupBy(d => new
                    {
                        d.Venta!.Fecha.Year,
                        d.Venta.Fecha.Month,
                        d.Venta.Fecha.Day
                    })
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = new DateTime(g.Key.Year, g.Key.Month, g.Key.Day),
                        TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario),
                        TipoAgrupacion = tipoAgrupacionString
                    }),

                GroupingType.Weekly => context.DetalleVentas!
                    .Include(d => d.Venta)
                    .Where(d => d.Venta!.KioscoId == kioscoId &&
                                d.Venta.Fecha >= fechaInicio &&
                                d.Venta.Fecha <= fechaFin)
                    .GroupBy(d => new
                    {
                        d.Venta!.Fecha.Year,
                        WeekStart = new DateTime(d.Venta.Fecha.Year, d.Venta.Fecha.Month, d.Venta.Fecha.Day)
                            .AddDays(-(int)d.Venta.Fecha.DayOfWeek + (d.Venta.Fecha.DayOfWeek == DayOfWeek.Sunday ? -6 : 1))
                    })
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = g.Key.WeekStart,
                        TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario),
                        TipoAgrupacion = tipoAgrupacionString
                    }),

                GroupingType.Monthly => context.DetalleVentas!
                    .Include(d => d.Venta)
                    .Where(d => d.Venta!.KioscoId == kioscoId &&
                                d.Venta.Fecha >= fechaInicio &&
                                d.Venta.Fecha <= fechaFin)
                    .GroupBy(d => new
                    {
                        d.Venta!.Fecha.Year,
                        d.Venta.Fecha.Month
                    })
                    .Select(g => new VentasPorDiaDto
                    {
                        Fecha = new DateTime(g.Key.Year, g.Key.Month, 1),
                        TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario),
                        TipoAgrupacion = tipoAgrupacionString
                    }),

                _ => throw new ArgumentOutOfRangeException(nameof(groupingType))
            };

            var ventasPorPeriodo = await query
                .OrderBy(v => v.Fecha)
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<VentasPorDiaDto>>.Success(ventasPorPeriodo);
        }

        public async Task<Result<IReadOnlyList<CategoriasRentabilidadDto>>> GetCategoriasRentabilidadAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            var detallesVentaPorCategoria = await context.DetalleVentas!
                .Include(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                           d.Venta.Fecha >= fechaInicio &&
                           d.Venta.Fecha <= fechaFin &&
                           d.Producto!.Categoria != null)
                .ToListAsync(cancellationToken);

            var categoriasDatos = detallesVentaPorCategoria
                .GroupBy(d => new { d.Producto!.CategoriaId, d.Producto.Categoria!.Nombre })
                .Select(g => new
                {
                    g.Key.CategoriaId,
                    g.Key.Nombre,
                    Detalles = g.ToList(),
                    TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario),
                })
                .ToList();

            decimal totalVentasGeneral = categoriasDatos.Sum(c => c.TotalVentas);
            var resultados = new List<CategoriasRentabilidadDto>();

            foreach (var categoria in categoriasDatos)
            {
                resultados.Add(new CategoriasRentabilidadDto
                {
                    CategoriaId = categoria.CategoriaId,
                    Nombre = categoria.Nombre,
                    PorcentajeVentas = totalVentasGeneral > 0 ? Math.Round(categoria.TotalVentas / totalVentasGeneral * 100, 2) : 0
                });
            }

            var categoriasOrdenadas = resultados
                .OrderByDescending(p => p.PorcentajeVentas)
                .ToList();

            return Result<IReadOnlyList<CategoriasRentabilidadDto>>.Success(categoriasOrdenadas);
        }


        private async Task<Dictionary<int, decimal?>> ObtenerCostosHistoricosAsync(
            IEnumerable<(int ProductoId, DateTime Fecha)> productosYFechas,
            CancellationToken cancellationToken)
        {
            var productosIds = productosYFechas.Select(p => p.ProductoId).Distinct().ToList();

            if (productosIds.Count == 0)
                return new Dictionary<int, decimal?>();

            var compraDetalles = await context.CompraDetalles!
                .Include(cd => cd.Compra)
                .Where(cd => productosIds.Contains(cd.ProductoId) && cd.Compra != null)
                .OrderByDescending(cd => cd.Compra!.Fecha)
                .ToListAsync(cancellationToken);

            var costosHistoricos = new Dictionary<int, decimal?>();

            foreach (var productoId in productosIds)
            {
                var fechaMasReciente = productosYFechas
                    .Where(p => p.ProductoId == productoId)
                    .Max(p => p.Fecha);

                var costoHistorico = compraDetalles
                    .Where(cd => cd.ProductoId == productoId &&
                                cd.Compra != null &&
                                cd.Compra.Fecha <= fechaMasReciente)
                    .OrderByDescending(cd => cd.Compra!.Fecha)
                    .FirstOrDefault()?.CostoUnitario;

                costosHistoricos[productoId] = costoHistorico;
            }

            return costosHistoricos;
        }

        private static GroupingType DetermineGroupingType(int rangeDays)
        {
            return rangeDays switch
            {
                <= 31 => GroupingType.Daily,
                <= 90 => GroupingType.Weekly,
                _ => GroupingType.Monthly
            };
        }

        private static string GetGroupingTypeString(GroupingType groupingType)
        {
            return groupingType switch
            {
                GroupingType.Daily => "daily",
                GroupingType.Weekly => "weekly",
                GroupingType.Monthly => "monthly",
                _ => "daily"
            };
        }
    }
}
