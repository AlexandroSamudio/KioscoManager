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
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(30);

        public ReporteRepository(DataContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        private static DateTime NormalizeToUtcDate(DateTime date)
        {
            return date.Kind == DateTimeKind.Utc ? date : DateTime.SpecifyKind(date, DateTimeKind.Utc);
        }

        public async Task<ReporteDto> CalculateKpiReporteAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default)
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

            decimal costoMercaderia = 0;

            foreach (var detalle in detallesVenta)
            {
                var costoHistorico = await ObtenerCostoHistoricoAsync(detalle.ProductoId, detalle.Venta!.Fecha, cancellationToken);

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
                .SetAbsoluteExpiration(TimeSpan.FromHours(12));

            _cache.Set(cacheKey, result, cacheOptions);

            return result;
        }

        private async Task<decimal?> ObtenerCostoHistoricoAsync(int productoId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            fecha = NormalizeToUtcDate(fecha);

            var compraDetalle = await _context.CompraDetalles!
                .Where(cd => cd.ProductoId == productoId && cd.Compra!.Fecha <= fecha)
                .OrderByDescending(cd => cd.Compra!.Fecha)
                .FirstOrDefaultAsync(cancellationToken);

            return compraDetalle?.CostoUnitario;
        }
        
        public async Task<PagedList<ProductoMasVendidoDto>> GetTopProductsByVentasAsync(
            int kioscoId,
            int pageNumber,
            int pageSize,
            DateTime fechaInicio,
            DateTime fechaFin,
            int limit = 5,
            CancellationToken cancellationToken = default)
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
                .SetAbsoluteExpiration(TimeSpan.FromHours(6));

            _cache.Set(cacheKey, pagedList, cacheOptions);

            return pagedList;
        }

        public async Task<IReadOnlyList<VentasPorDiaDto>> GetVentasPorDiaAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default)
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

            foreach (var ventaDia in ventasPorDiaSinGanancia)
            {
                // Obtener el día específico para filtrar
                var fechaDia = ventaDia.Fecha;
                var finDia = NormalizeToUtcDate(fechaDia.AddHours(23).AddMinutes(59).AddSeconds(59));
                
                // Obtener todos los detalles de venta de ese día
                var detallesVenta = await _context.DetalleVentas!
                    .Include(d => d.Producto)
                    .Include(d => d.Venta)
                    .Where(d => d.Venta!.KioscoId == kioscoId && 
                               d.Venta.Fecha >= fechaDia && 
                               d.Venta.Fecha <= finDia)
                    .ToListAsync(cancellationToken);

                decimal costoMercaderia = 0;
                foreach (var detalle in detallesVenta)
                {
                    var costoHistorico = await ObtenerCostoHistoricoAsync(
                        detalle.ProductoId, 
                        detalle.Venta!.Fecha, 
                        cancellationToken);

                    decimal costoPorUnidad = costoHistorico ?? detalle.Producto!.PrecioCompra;
                    costoMercaderia += detalle.Cantidad * costoPorUnidad;
                }
            }

            var cacheOptionsWithProfit = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromHours(4));

            _cache.Set(cacheKey, ventasPorDiaSinGanancia, cacheOptionsWithProfit);
            
            return ventasPorDiaSinGanancia;
        }

        public async Task<IReadOnlyList<CategoriasRentabilidadDto>> GetCategoriasRentabilidadAsync(
            int kioscoId,
            DateTime fechaInicio,
            DateTime fechaFin,
            CancellationToken cancellationToken = default)
        {
            // Asegurar que la fecha de fin incluya todo el día
            var endOfDay = NormalizeToUtcDate(new DateTime(fechaFin.Year, fechaFin.Month, fechaFin.Day, 23, 59, 59));
            string cacheKey = $"categoriasRentabilidad_{kioscoId}_{fechaInicio:yyyyMMdd}_{fechaFin:yyyyMMdd}";

            // Intentar obtener del caché
            if (_cache.TryGetValue(cacheKey, out IReadOnlyList<CategoriasRentabilidadDto>? cachedResult) && cachedResult != null)
            {
                return cachedResult;
            }

            // Obtenemos los detalles de venta agrupados por categoría
            var detallesVentaPorCategoria = await _context.DetalleVentas!
                .Include(d => d.Producto)
                    .ThenInclude(p => p!.Categoria)
                .Include(d => d.Venta)
                .Where(d => d.Venta!.KioscoId == kioscoId && 
                           d.Venta.Fecha >= fechaInicio && 
                           d.Venta.Fecha <= endOfDay &&
                           d.Producto!.Categoria != null)
                .ToListAsync(cancellationToken);

            // Agrupar por categoría
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

            // Calcular costos y ganancias para cada categoría
            foreach (var categoria in categoriasDatos)
            {
                resultados.Add(new CategoriasRentabilidadDto
                {
                    CategoriaId = categoria.CategoriaId,
                    Nombre = categoria.Nombre,
                    PorcentajeVentas = totalVentasGeneral > 0 ? Math.Round((categoria.TotalVentas / totalVentasGeneral) * 100, 2) : 0
                });
            }

            var categoriasFinal = resultados
                .OrderByDescending(p => p.PorcentajeVentas)
                .ToList();

            var cacheOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(_cacheDuration)
                .SetAbsoluteExpiration(TimeSpan.FromHours(4));

            _cache.Set(cacheKey, categoriasFinal, cacheOptions);
            
            return categoriasFinal;
        }
    }
}
