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

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del proveedor debe tener entre 3 y 50 caracteres.")]
        public string? Proveedor { get; set; }
        [StringLength(100, MinimumLength = 3, ErrorMessage = "La nota debe tener entre 3 y 100 caracteres.")]
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

        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del producto debe tener entre 3 y 50 caracteres.")]
        public string? ProductoNombre { get; set; }
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El SKU del producto debe tener entre 3 y 50 caracteres.")]
        public string? ProductoSku { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public required int Cantidad { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor que cero")]
        public required decimal CostoUnitario { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "El subtotal debe ser mayor que cero")]
        public required decimal Subtotal { get; set; }
    }

    public class CompraCreateDto
    {
        [Required]
        public required ICollection<CompraDetalleDto> Detalles { get; set; } = new List<CompraDetalleDto>();
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
