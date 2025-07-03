using API.DTOs;
using API.Helpers;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace API.Data.Repositories
{
    public class ReporteRepository : IReporteRepository
    {
        private readonly DataContext _context;
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration;
        private readonly TimeSpan _absoluteExpiration;

        public ReporteRepository(
            DataContext context, 
            IMemoryCache cache, 
            IConfiguration configuration)
        {
            _context = context;
            _cache = cache;
            
            _cacheDuration = TimeSpan.FromMinutes(
                configuration.GetValue<int>("Reporting:CacheDurationMinutes", 30));
            
            _absoluteExpiration = TimeSpan.FromHours(
                configuration.GetValue<int>("Reporting:AbsoluteExpirationHours", 4));
        }

        private static DateTime NormalizeToUtcDate(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        public async Task<ReporteDto> CalculateKpiReporteAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            fechaInicio = NormalizeToUtcDate(fechaInicio);
            fechaFin = NormalizeToUtcDate(fechaFin);

            var endOfDay = NormalizeToUtcDate(
                fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            var cacheKey = $"KPIReporte_K{kioscoId}_S{fechaInicio:yyyyMMdd}_E{endOfDay:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out ReporteDto? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var ventasQuery = _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha <= endOfDay);

            var numeroTransacciones = await ventasQuery.CountAsync(cancellationToken);

            var totalVentas = await ventasQuery
                .SumAsync(v => v.Total, cancellationToken);

            var detallesVenta = await _context.DetalleVentas!
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId && d.Venta.Fecha >= fechaInicio && d.Venta.Fecha <= endOfDay)
                .ToListAsync(cancellationToken);

            var productosYFechas = detallesVenta
                .Select(d => (d.ProductoId, d.Venta!.Fecha))
                .Distinct()
                .ToList();

            var costosHistoricos = await ObtenerCostosHistoricosBulkAsync(productosYFechas, cancellationToken);

            decimal costoMercaderia = 0;

            foreach (var detalle in detallesVenta)
            {
                var costoHistorico = costosHistoricos.TryGetValue(detalle.ProductoId, out var costo) ? costo : null;
                decimal costoPorUnidad = costoHistorico ?? detalle.Producto!.PrecioCompra;
                costoMercaderia += detalle.Cantidad * costoPorUnidad;
            }

            decimal gananciaBruta = totalVentas - costoMercaderia;

            var result = new ReporteDto
            {
                TotalVentas = totalVentas,
                CostoMercaderiaVendida = costoMercaderia,
                GananciaBruta = gananciaBruta,
                NumeroTransacciones = numeroTransacciones,
                FechaInicio = fechaInicio,
                FechaFin = fechaFin
            };

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(_absoluteExpiration);

            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }

        public async Task<PagedList<ProductoMasVendidoDto>> GetTopProductsByVentasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken,
            int limit = 5)
        {
            fechaInicio = NormalizeToUtcDate(fechaInicio);
            fechaFin = NormalizeToUtcDate(fechaFin);

            limit = Math.Clamp(limit, 1, 50);

            var endOfDay = NormalizeToUtcDate(
                fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            var cacheKey = $"TopProducts_K{kioscoId}_S{fechaInicio:yyyyMMdd}_E{endOfDay:yyyyMMdd}_L{limit}_P{pageNumber}_S{pageSize}";

            if (_cache.TryGetValue(cacheKey, out PagedList<ProductoMasVendidoDto>? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var query = _context.DetalleVentas!
                .Include(d => d.Producto)
                .Include(d => d.Producto!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                           d.Venta.Fecha >= fechaInicio &&
                           d.Venta.Fecha <= endOfDay)
                .GroupBy(d => new
                {
                    d.ProductoId,
                    Nombre = d.Producto!.Nombre,
                    Sku = d.Producto.Sku,
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
                .OrderByDescending(p => p.CantidadVendida)
                .Take(limit);

            var pagedList = await PagedList<ProductoMasVendidoDto>.CreateAsync(
                query.AsNoTracking(), pageNumber, pageSize, cancellationToken);

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(_absoluteExpiration);

            _cache.Set(cacheKey, pagedList, cacheOptions);

            return pagedList;
        }

        public async Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            fechaInicio = NormalizeToUtcDate(fechaInicio);
            fechaFin = NormalizeToUtcDate(fechaFin);

            var endOfDay = NormalizeToUtcDate(
                fechaFin.Date.AddHours(23).AddMinutes(59).AddSeconds(59));

            var cacheKey = $"VentasPorDia_K{kioscoId}_S{fechaInicio:yyyyMMdd}_E{endOfDay:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<VentasPorDiaDto>? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var ventasPorDiaSinGanancia = await _context.DetalleVentas!
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                          d.Venta.Fecha >= fechaInicio &&
                          d.Venta.Fecha <= endOfDay)
                .GroupBy(d => d.Venta!.Fecha.Date)
                .Select(g => new VentasPorDiaDto
                {
                    Fecha = g.Key,
                    TotalVentas = g.Sum(d => d.Cantidad * d.PrecioUnitario),
                })
                .OrderBy(v => v.Fecha)
                .ToListAsync(cancellationToken);

            var todosLosDetalles = await _context.DetalleVentas!
                .Include(d => d.Producto)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                           d.Venta.Fecha >= fechaInicio &&
                           d.Venta.Fecha <= endOfDay)
                .ToListAsync(cancellationToken);

            var productosYFechas = todosLosDetalles
                .Select(d => (d.ProductoId, d.Venta!.Fecha))
                .Distinct()
                .ToList();

            var costosHistoricos = await ObtenerCostosHistoricosBulkAsync(productosYFechas, cancellationToken);

            foreach (var ventaDia in ventasPorDiaSinGanancia)
            {
                var fechaDia = ventaDia.Fecha;
                var finDia = NormalizeToUtcDate(fechaDia.AddHours(23).AddMinutes(59).AddSeconds(59));

                var detallesDelDia = todosLosDetalles
                    .Where(d => d.Venta!.Fecha >= fechaDia && d.Venta.Fecha <= finDia)
                    .ToList();

                decimal costoMercaderia = 0;
                foreach (var detalle in detallesDelDia)
                {
                    var costoHistorico = costosHistoricos.TryGetValue(detalle.ProductoId, out var costo) ? costo : null;
                    decimal costoPorUnidad = costoHistorico ?? detalle.Producto!.PrecioCompra;
                    costoMercaderia += detalle.Cantidad * costoPorUnidad;
                }
            }

            var cacheOptionsWithProfit = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(_absoluteExpiration);

            _cache.Set(cacheKey, ventasPorDiaSinGanancia, cacheOptionsWithProfit);

            return ventasPorDiaSinGanancia;
        }

        public async Task<IReadOnlyList<CategoriasRentabilidadDto>> GetCategoriasRentabilidadAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken)
        {
            var endOfDay = NormalizeToUtcDate(new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59));
            string cacheKey = $"categoriasRentabilidad_{kioscoId}_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}";

            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CategoriasRentabilidadDto>? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            var detallesVentaPorCategoria = await _context.DetalleVentas!
                .Include(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId &&
                           d.Venta.Fecha >= fechaInicio &&
                           d.Venta.Fecha <= endOfDay &&
                           d.Producto!.Categoria != null)
                .ToListAsync(cancellationToken);

            var categoriasDatos = detallesVentaPorCategoria
                .GroupBy(d => new { d.Producto!.CategoriaId, d.Producto.Categoria!.Nombre })
                .Select(g => new
                {
                    CategoriaId = g.Key.CategoriaId,
                    Nombre = g.Key.Nombre,
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

            var categoriasFinal = resultados
                .OrderByDescending(p => p.PorcentajeVentas)
                .ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(_absoluteExpiration);

            _cache.Set(cacheKey, categoriasFinal, cacheOptions);

            return categoriasFinal;
        }


        private async Task<Dictionary<int, decimal?>> ObtenerCostosHistoricosBulkAsync(
            IEnumerable<(int ProductoId, DateTime Fecha)> productosYFechas,
            CancellationToken cancellationToken)
        {
            var productosIds = productosYFechas.Select(p => p.ProductoId).Distinct().ToList();

            if (productosIds.Count == 0)
                return new Dictionary<int, decimal?>();

            var compraDetalles = await _context.CompraDetalles!
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

                var fechaNormalizada = NormalizeToUtcDate(fechaMasReciente);

                var costoHistorico = compraDetalles
                    .Where(cd => cd.ProductoId == productoId &&
                                cd.Compra != null &&
                                cd.Compra.Fecha <= fechaNormalizada)
                    .OrderByDescending(cd => cd.Compra!.Fecha)
                    .FirstOrDefault()?.CostoUnitario;

                costosHistoricos[productoId] = costoHistorico;
            }

            return costosHistoricos;
        }
    }
}
