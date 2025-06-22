using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CompraCreateDto
    {
        [Required]
        public required ICollection<CompraDetalleDto> Productos { get; set; } = new List<CompraDetalleDto>();
        public string? Proveedor { get; set; }
        public string? Nota { get; set; }
    }

    public class CompraDetalleDto
    {
        [Required]
        public required int ProductoId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public required int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor que cero")]
        public required decimal CostoUnitario { get; set; }
    }
}
