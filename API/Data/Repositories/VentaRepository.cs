using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using API.Constants;
using System.Runtime.CompilerServices;

namespace API.Data.Repositories
{
    public class VentaRepository(DataContext context, IMapper mapper) : IVentaRepository
    {
        public async Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaCreateDto, int kioscoId, int usuarioId, CancellationToken cancellationToken)
        {
            var productosIds = ventaCreateDto.Productos.Select(p => p.ProductoId).Distinct().ToList();
            var productos = await context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                return Result<VentaDto>.Failure(ErrorCodes.EntityNotFound, $"Los siguientes id de productos no se encontraron en el kiosco: {string.Join(", ", noEncontrados)}");
            }

            var cantidadesPorProducto = new Dictionary<int, int>();
            foreach (var detalle in ventaCreateDto.Productos)
            {
                if (!cantidadesPorProducto.TryAdd(detalle.ProductoId, detalle.Cantidad))
                {
                    cantidadesPorProducto[detalle.ProductoId] += detalle.Cantidad;
                }
            }

            foreach (var kvp in cantidadesPorProducto)
            {
                var productoId = kvp.Key;
                var cantidadSolicitada = kvp.Value;
                var producto = productos[productoId];
                if (producto.Stock < cantidadSolicitada)
                {
                    var errorMessage = $"Stock insuficiente para '{producto.Nombre}'. Solicitado: {cantidadSolicitada}, Disponible: {producto.Stock}.";
                    return Result<VentaDto>.Failure(ErrorCodes.InsufficientStock, errorMessage);
                }
            }

            await using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var venta = mapper.Map<Venta>(ventaCreateDto);
                venta.KioscoId = kioscoId;
                venta.UsuarioId = usuarioId;
                venta.Fecha = DateTime.UtcNow;

                foreach (var detalle in venta.Detalles)
                {
                    var producto = productos[detalle.ProductoId];
                    detalle.PrecioUnitario = producto.PrecioVenta;
                    producto.Stock -= detalle.Cantidad;
                }

                venta.Total = venta.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);

                context.Ventas!.Add(venta);
                await context.SaveChangesAsync(cancellationToken);

                await transaction.CommitAsync(cancellationToken);

                var ventaDto = mapper.Map<VentaDto>(venta);
                return Result<VentaDto>.Success(ventaDto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<VentaDto>.Failure(ErrorCodes.InvalidOperation, "Ocurrió un error al crear la venta.");
            }
        }

        public async Task<Result<IReadOnlyList<VentaDto>>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            var baseDate = fecha?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone).Date;
            var fechaInicio = baseDate.Kind == DateTimeKind.Utc ? baseDate : DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
            var fechaFin = fechaInicio.AddDays(1);

            var ventas = await context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha < fechaFin)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<VentaDto>>.Success(ventas);
        }

        public async Task<Result<IReadOnlyList<VentaDto>>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken)
        {
            var ventas = await context.Ventas!
                .Where(v => v.KioscoId == kioscoId)
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<VentaDto>>.Success(ventas);
        }

        public async Task<Result<decimal>> GetMontoTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            var baseDate = fecha?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone).Date;
            var fechaInicio = baseDate.Kind == DateTimeKind.Utc ? baseDate : DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
            var fechaFin = fechaInicio.AddDays(1);

            var montoVentas = await context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);

            return Result<decimal>.Success(montoVentas);
        }

        public async Task<Result<IReadOnlyList<ProductoMasVendidoDto>>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            var baseDate = fecha?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone).Date;
            var fechaInicio = baseDate.Kind == DateTimeKind.Utc ? baseDate : DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
            var fechaFin = fechaInicio.AddDays(1);

            var productos = await context.DetalleVentas!
                .AsNoTracking()
                .Where(dv => dv.Venta!.KioscoId == kioscoId && dv.Venta!.Fecha >= fechaInicio && dv.Venta.Fecha < fechaFin)
                .GroupBy(dv => new { dv.ProductoId, dv.Producto!.Nombre})
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    CantidadVendida = g.Sum(dv => dv.Cantidad),
                    TotalVentas = g.Sum(dv => dv.Cantidad * dv.PrecioUnitario),
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<ProductoMasVendidoDto>>.Success(productos);
        }

        public Result<IAsyncEnumerable<VentaDto>> GetVentasForExport(
            int kioscoId,
            CancellationToken cancellationToken,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? limite = null)
        {
            var stream = GetVentasForExportInternalAsync(kioscoId, cancellationToken, fechaInicio, fechaFin, limite);
            return Result<IAsyncEnumerable<VentaDto>>.Success(stream);
        }

        private async IAsyncEnumerable<VentaDto> GetVentasForExportInternalAsync(
            int kioscoId,
            [EnumeratorCancellation] CancellationToken cancellationToken,
            DateTime? fechaInicio = null,
            DateTime? fechaFin = null,
            int? limite = null)
        {
            var query = context.Ventas!
                .Where(v => v.KioscoId == kioscoId)
                .AsNoTracking()
                .AsQueryable();

            if (fechaInicio.HasValue)
            {
                var fechaInicioUtc = fechaInicio.Value.Kind == DateTimeKind.Utc
                    ? fechaInicio.Value
                    : fechaInicio.Value.ToUniversalTime();
                query = query.Where(v => v.Fecha >= fechaInicioUtc);
            }

            if (fechaFin.HasValue)
            {
                var fechaFinUtc = fechaFin.Value.Kind == DateTimeKind.Utc
                    ? fechaFin.Value
                    : fechaFin.Value.ToUniversalTime();
                query = query.Where(v => v.Fecha <= fechaFinUtc);
            }

            query = query.OrderByDescending(v => v.Fecha);
            if (limite.HasValue)
            {
                query = query.Take(limite.Value);
            }

            var ventas = query
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .AsAsyncEnumerable();

            await foreach (var venta in ventas.WithCancellation(cancellationToken))
            {
                yield return venta;
            }
        }

        public async Task<Result<IReadOnlyList<VentaChartDto>>> GetVentasIndividualesDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var argentinaTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Argentina/Buenos_Aires");
            var baseDate = fecha?.Date ?? TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, argentinaTimeZone).Date;
            var fechaInicio = baseDate.Kind == DateTimeKind.Utc ? baseDate : DateTime.SpecifyKind(baseDate, DateTimeKind.Utc);
            var fechaFin = fechaInicio.AddDays(1);

            var ventas = await context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha < fechaFin)
                .OrderBy(v => v.Fecha)
                .AsNoTracking()
                .Select(v => new VentaChartDto
                {
                    Id = v.Id,
                    Fecha = v.Fecha,
                    Total = v.Total
                })
                .ToListAsync(cancellationToken);

            return Result<IReadOnlyList<VentaChartDto>>.Success(ventas);
        }
    }
}
