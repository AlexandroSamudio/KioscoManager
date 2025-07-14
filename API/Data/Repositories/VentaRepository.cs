using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class VentaRepository(DataContext context, IMapper mapper) : IVentaRepository
    {
        private readonly DataContext _context = context;
        private readonly IMapper _mapper = mapper;

        public async Task<Result<VentaDto>> CreateVentaAsync(VentaCreateDto ventaCreateDto, int kioscoId, int usuarioId, CancellationToken cancellationToken = default)
        {
            var productosIds = ventaCreateDto.Productos.Select(p => p.ProductoId).Distinct().ToList();
            var productos = await _context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                var errorMessage = $"Los siguientes productos no se encontraron: {string.Join(", ", noEncontrados)}";
                return Result<VentaDto>.Failure(VentaErrorCodes.ProductosNoEncontrados, errorMessage);
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            
            try
            {
                var venta = _mapper.Map<Venta>(ventaCreateDto);
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

                _context.Ventas!.Add(venta);
                await _context.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                
                var ventaDto = _mapper.Map<VentaDto>(venta);
                return Result<VentaDto>.Success(ventaDto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                return Result<VentaDto>.Failure(VentaErrorCodes.ErrorDeCreacion, "Ocurrió un error al crear la venta.");
            }
        }

        public async Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId,CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken)
        {
            return await _context.Ventas!
                .Where(v => v.KioscoId == kioscoId)
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public async Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken, DateTime? fecha = null)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await _context.DetalleVentas!
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
    }
}
