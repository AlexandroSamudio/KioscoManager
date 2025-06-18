using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoCreateDto
    {
        [StringLength(100)]
        public required string Nombre { get; set; }

        public required int CategoriaId { get; set; }

        [Range(0.01, 1000000)]
        public required decimal PrecioCompra { get; set; }

        [Range(0.01, 1000000)]
        public required decimal PrecioVenta { get; set; }

        [Range(0, 1000000)]
        public required int Stock { get; set; }

        public string? Descripcion { get; set; }

        [StringLength(50)]
        public required string Sku { get; set; }
    }
}
