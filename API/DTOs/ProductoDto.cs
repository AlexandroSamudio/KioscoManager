using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoDto
    {
        public required int Id { get; set; }
        public required string Sku { get; set; } = default!;
        [Required, StringLength(100)]
        public required string Nombre { get; set; } = default!;
        public string? Descripcion { get; set; }
        [Range(0.01, 1000000)]
        public required decimal PrecioCompra { get; set; }
        [Range(0.01, 1000000)]
        public required decimal PrecioVenta { get; set; }
        [Range(0, 1000000)]
        public required int Stock { get; set; }
        [Required, StringLength(100)]
        public required string CategoriaNombre { get; set; } = default!;
    }
}