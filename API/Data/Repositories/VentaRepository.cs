using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using API.Entities;
using Microsoft.EntityFrameworkCore;

namespace API.Data.Repositories
{
    public class VentaRepository : IVentaRepository
    {
        private readonly DataContext _context;
        private readonly IMapper _mapper;

        public VentaRepository(DataContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<Venta> CreateVentaAsync(VentaCreateDto ventaData, int kioscoId, int usuarioId, CancellationToken cancellationToken = default)
        {
            var productosIds = ventaData.Productos.Select(p => p.ProductoId).Distinct().ToList();
            var productos = await _context.Productos!
                .Where(p => p.KioscoId == kioscoId && productosIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p, cancellationToken);

            if (productos.Count != productosIds.Count)
            {
                var noEncontrados = productosIds.Except(productos.Keys).ToList();
                throw new ArgumentException($"Los siguientes productos no se encontraron en el kiosco: {string.Join(", ", noEncontrados)}");
            }

            using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            try
            {
                var venta = new Venta
                {
                    KioscoId = kioscoId,
                    UsuarioId = usuarioId,
                    Fecha = DateTime.UtcNow
                };

                foreach (var productoVenta in ventaData.Productos)
                {
                    var producto = productos[productoVenta.ProductoId];
                    var detalle = new DetalleVenta
                    {
                        ProductoId = producto.Id,
                        Cantidad = productoVenta.Cantidad,
                        PrecioUnitario = producto.PrecioVenta
                    };
                    
                    venta.Detalles.Add(detalle);
                }

                venta.Total = venta.Detalles.Sum(d => d.PrecioUnitario * d.Cantidad);

                _context.Ventas!.Add(venta);
                
                foreach (var detalle in venta.Detalles)
                {
                    var producto = productos[detalle.ProductoId];
                    if (producto != null)
                    {
                        if (producto.Stock < detalle.Cantidad)
                        {
                            throw new InvalidOperationException($"Stock insuficiente para el producto {producto.Nombre}. Stock actual: {producto.Stock}, Cantidad solicitada: {detalle.Cantidad}");
                        }
                        
                        producto.Stock -= detalle.Cantidad;
                    }
                }

                await _context.SaveChangesAsync(cancellationToken);
                
                await transaction.CommitAsync(cancellationToken);
                
                return venta;
            }
            catch (Exception)
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
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
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }

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
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }

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
