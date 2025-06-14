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

        public async Task<IReadOnlyList<VentaDto>> GetVentasDelDiaAsync(int kioscoId, DateTime? fecha = null, CancellationToken cancellationToken = default)
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

        public async Task<IReadOnlyList<VentaDto>> GetVentasRecientesAsync(int kioscoId, int cantidad, CancellationToken cancellationToken = default)
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

        public async Task<decimal> GetTotalVentasDelDiaAsync(int kioscoId, DateTime? fecha = null, CancellationToken cancellationToken = default)
        {
            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await _context.Ventas!
                .Where(v => v.KioscoId == kioscoId && v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);
        }

        public async Task<IReadOnlyList<ProductoMasVendidoDto>> GetProductosMasVendidosDelDiaAsync(int kioscoId, int cantidad, DateTime? fecha = null, CancellationToken cancellationToken = default)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }

            var fechaConsulta = fecha ?? DateTime.UtcNow.Date;
            var fechaUtc = fechaConsulta.Kind == DateTimeKind.Utc ? fechaConsulta.Date : fechaConsulta.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return await _context.DetalleVentas!
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
                .AsNoTracking()
                .ToListAsync(cancellationToken);
        }
    }
}
