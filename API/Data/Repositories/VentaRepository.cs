using API.DTOs;
using API.Interfaces;
using AutoMapper;
using AutoMapper.QueryableExtensions;
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

        public Task<List<VentaDto>> GetVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken = default)
        {
            return GetVentasDelDiaAsync(kioscoId, DateTime.UtcNow.Date, cancellationToken);
        }

        public Task<List<VentaDto>> GetVentasDelDiaAsync(int kioscoId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaUtc = fecha.Kind == DateTimeKind.Utc ? fecha.Date : fecha.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public Task<List<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default)
        {
            return _context.Ventas!
                .Where(v => v.KioscoId == kioscoId)
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, CancellationToken cancellationToken = default)
        {
            return GetTotalVentasDelDiaAsync(kioscoId, DateTime.UtcNow.Date, cancellationToken);
        }

        public Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaUtc = fecha.Kind == DateTimeKind.Utc ? fecha.Date : fecha.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);
        }

        public Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default)
        {
            return GetProductosMasVendidosDelDiaAsync(kioscoId, DateTime.UtcNow.Date, cantidad, cancellationToken);
        }

        public Task<List<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, DateTime fecha, int cantidad, CancellationToken cancellationToken = default)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }

            var fechaUtc = fecha.Kind == DateTimeKind.Utc ? fecha.Date : fecha.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return _context.DetalleVentas!
                .Where(dv => dv.Venta!.KioscoId == kioscoId && dv.Venta!.Fecha >= fechaUtc && dv.Venta.Fecha < fechaFin)
                .GroupBy(dv => new { dv.ProductoId, dv.Producto!.Nombre, dv.PrecioUnitario })
                .Select(g => new ProductoMasVendidoDto
                {
                    ProductoId = g.Key.ProductoId,
                    NombreProducto = g.Key.Nombre,
                    CantidadVendida = g.Sum(dv => dv.Cantidad),
                })
                .OrderByDescending(p => p.CantidadVendida)
                .Take(cantidad)
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
