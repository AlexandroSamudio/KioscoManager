using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs;

public class ReporteDto
{
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal TotalVentas { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal CostoMercaderiaVendida { get; set; }

    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public required decimal GananciaBruta { get; set; }

    [Required]
    public required int NumeroTransacciones { get; set; }

    [Required]
    public required DateTime FechaInicio { get; set; }
    [Required]
    public required DateTime FechaFin { get; set; }
}
