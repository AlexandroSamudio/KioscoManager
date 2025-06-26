using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoMasVendidoDto
    {
        [Required]
        public required int ProductoId { get; set; }
        [Required]
        public required string NombreProducto { get; set; }
        public string? Sku { get; set; }
        [Required]
        public required int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public string? CategoriaNombre { get; set; }
    }
}
