namespace API.DTOs
{
    public class CompraDto
    {
        public int Id { get; set; }
        public required DateTime Fecha { get; set; }
        public required decimal CostoTotal { get; set; }
        public string? Proveedor { get; set; }
        public string? Nota { get; set; }
        public required int UsuarioId { get; set; }
        public required ICollection<CompraDetalleViewDto> Detalles { get; set; } = new List<CompraDetalleViewDto>();
    }

    public class CompraDetalleViewDto
    {
        public int Id { get; set; }
        public required int ProductoId { get; set; }
        public string? ProductoNombre { get; set; }
        public string? ProductoSku { get; set; }
        public required int Cantidad { get; set; }
        public required decimal CostoUnitario { get; set; }
        public required decimal Subtotal { get; set; }
    }

    public class CompraCreateDto
    {
        public IReadOnlyList<CompraDetalleDto> Detalles { get; set; } = [];
        public string? Proveedor { get; set; }
        public string? Nota { get; set; }
    }

    public class CompraDetalleDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
