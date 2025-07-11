using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs
{
    public class VentaDto
    {
        [Required]
        public required int Id { get; set; }
        [Required]
        public required DateTime Fecha { get; set; }
        [Required]
        public required decimal Total { get; set; }
        [Required]
        public required int CantidadProductos { get; set; }
    }

    public class VentaCreateDto
    {
        [Required]
        public required List<ProductoVentaDto> Productos { get; set; }
    }

    public class ProductoVentaDto
    {
        [Required]
        public required int ProductoId { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
        public required int Cantidad { get; set; }
    }

    public class VentasPorDiaDto
    {
        [Required]
        public required DateTime Fecha { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal TotalVentas { get; set; }
    }
}
