using System.ComponentModel.DataAnnotations;

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
}
