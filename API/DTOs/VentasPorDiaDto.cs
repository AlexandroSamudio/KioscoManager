using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs
{
    public class VentasPorDiaDto
    {
        [Required]
        public required DateTime Fecha { get; set; }
        
        [Required]
        [Column(TypeName = "decimal(18, 2)")]
        public required decimal TotalVentas { get; set; }
    }
}
