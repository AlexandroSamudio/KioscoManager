using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CompraDto
    {
        public int Id { get; set; }
        [Required]
        public required DateTime Fecha { get; set; }

        [Required]
        public required decimal CostoTotal { get; set; }

        public string? Proveedor { get; set; }
        public string? Nota { get; set; }

        [Required]
        public required int UsuarioId { get; set; }

        [Required]
        public required ICollection<CompraDetalleViewDto> Detalles { get; set; } = new List<CompraDetalleViewDto>();
    }

    public class CompraDetalleViewDto
    {
        public int Id { get; set; }

        [Required]
        public required int ProductoId { get; set; }

        public string? ProductoNombre { get; set; }
        public string? ProductoSku { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public required int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor que cero")]
        public required decimal CostoUnitario { get; set; }

        [Required]
        public required decimal Subtotal { get; set; }
    }
}
