using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class ProductoDto
    {
        public required int Id { get; set; }
        [Required, StringLength(100)]
        public required string Nombre { get; set; } = default!;
        public required string? Descripcion { get; set; }
        [Range(0.01, 1000000)]
        public required decimal Precio { get; set; }
        [Range(0, 1000000)]
        public required int Stock { get; set; }
        [Required, StringLength(100)]
        public required string CategoriaNombre { get; set; } = default!;
    }
}
