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

        public Task<List<VentaDto>> GetVentasDelDiaAsync(CancellationToken cancellationToken = default)
        {
            return GetVentasDelDiaAsync(DateTime.UtcNow.Date, cancellationToken);
        }

        public Task<List<VentaDto>> GetVentasDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaUtc = fecha.Kind == DateTimeKind.Utc ? fecha.Date : fecha.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return _context.Ventas!
                .Where(v => v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .OrderByDescending(v => v.Fecha)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public Task<List<VentaDto>> GetVentasRecientesAsync(int cantidad, CancellationToken cancellationToken = default)
        {
            if (cantidad <= 0)
            {
                throw new ArgumentOutOfRangeException(nameof(cantidad), "La cantidad debe ser mayor que cero.");
            }
            
            return _context.Ventas!
                .OrderByDescending(v => v.Fecha)
                .Take(cantidad)
                .AsNoTracking()
                .ProjectTo<VentaDto>(_mapper.ConfigurationProvider)
                .ToListAsync(cancellationToken);
        }

        public Task<decimal> GetTotalVentasDelDiaAsync(CancellationToken cancellationToken = default)
        {
            return GetTotalVentasDelDiaAsync(DateTime.UtcNow.Date, cancellationToken);
        }

        public Task<decimal> GetTotalVentasDelDiaAsync(DateTime fecha, CancellationToken cancellationToken = default)
        {
            var fechaUtc = fecha.Kind == DateTimeKind.Utc ? fecha.Date : fecha.ToUniversalTime().Date;
            var fechaFin = fechaUtc.AddDays(1);

            return _context.Ventas!
                .Where(v => v.Fecha >= fechaUtc && v.Fecha < fechaFin)
                .AsNoTracking()
                .SumAsync(v => v.Total, cancellationToken);
        }
    }
}
