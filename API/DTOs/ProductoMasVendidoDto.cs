using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoMasVendidoDto
    {
        [Required]
        public required int ProductoId { get; set; }
        [Required]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del producto debe tener entre 3 y 50 caracteres.")]
        public required string NombreProducto { get; set; }
            
        public string? Sku { get; set; }
        [Required]
        public required int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre de la categoría debe tener entre 3 y 50 caracteres.")]
        public string? CategoriaNombre { get; set; }
    }
}
