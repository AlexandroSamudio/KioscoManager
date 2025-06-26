using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CategoriasRentabilidadDto
    {
        [Required]
        public required int CategoriaId { get; set; }
        [Required]
        public required string Nombre { get; set; }
        [Required]
        public decimal PorcentajeVentas { get; set; }
    }
}
