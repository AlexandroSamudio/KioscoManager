using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Entities;
using Microsoft.EntityFrameworkCore;
using API.Constants;
using API.Helpers;

namespace API.Data.Repositories
{
    public class VentaRepository(DataContext context, IMapper mapper) : IVentaRepository
    {
        public async Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaCreateDto, int kioscoId, int usuarioId, CancellationToken cancellationToken = default)
        {
            var productosIds = ventaCreateDto.Productos.Select(p => p.ProductoId).Distinct().ToList();
            var productos = await context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                var errorMessage = $"Los siguientes productos no se encontraron: {string.Join(", ", noEncontrados)}";
                return Result<VentaDto>.Failure(VentaErrorCodes.ProductosNoEncontrados, errorMessage);
            }

            using var transaction = await context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var venta = mapper.Map<Venta>(ventaCreateDto);
                venta.KioscoId = kioscoId;
                venta.UsuarioId = usuarioId;
                venta.Fecha = DateTime.UtcNow;

                foreach (var detalle in venta.Detalles)
                {
                    var producto = productos[detalle.ProductoId];
                    if (producto.Stock < detalle.Cantidad)
                    {
                        var errorMessage = $"Stock insuficiente para '{producto.Nombre}'. Solicitado: {detalle.Cantidad}, Disponible: {producto.Stock}.";
                        return Result<VentaDto>.Failure(VentaErrorCodes.StockInsuficiente, errorMessage);
                    }

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
                return Result<VentaDto>.Failure(VentaErrorCodes.ErrorDeCreacion, "Ocurrió un error al crear la venta.");
            }
        }

        public async Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaInicio = fecha?.Date ?? DateTime.UtcNow.Date;
            var fechaFin = fechaInicio.AddDays(1);

            return await context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaInicio && v.Fecha < fechaFin)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken)
        {
            return await context.Ventas!
                .Where(v => v.KioscoId == kioscoId)
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await context.DetalleVentas!
                .AsNoTracking()
                .Where(dv => dv.Venta!.KioscoId == kioscoId && dv.Venta!.Fecha >= fechaUtc && dv.Venta.Fecha < fechaFin)
                .GroupBy(dv => new { dv.ProductoId, dv.Producto!.Nombre})
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    CantidadVendida = g.Sum(dv => dv.Cantidad),
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<VentaDto>> GetVentasForExportAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fechaInicio = null, DateTime? fechaFin = null, int? limite = null)
        {
            const int DEFAULT_EXPORT_LIMIT = 5000;
            var limiteAplicar = limite ?? DEFAULT_EXPORT_LIMIT;

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

            return await query
                .OrderByDescending(v => v.Fecha)
                .Take(limiteAplicar)
                .ProjectTo<VentaDto>(mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }
    }
}
