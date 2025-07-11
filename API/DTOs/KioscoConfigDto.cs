using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs;

public class KioscoConfigDto
{
    public int Id { get; set; }
    public int KioscoId { get; set; }
    
    [Required]
    [StringLength(3, MinimumLength = 3, ErrorMessage = "La moneda debe tener exactamente 3 caracteres.")]
    public required string Moneda { get; set; }
    
    [Range(0, 100)]
    public decimal ImpuestoPorcentaje { get; set; }
    
    [StringLength(10, MinimumLength = 3, ErrorMessage = "El SKU debe tener entre 3 y 10 caracteres.")]
    public string? PrefijoSku { get; set; }
    
    [Range(1, 1000)]
    public int StockMinimoDefault { get; set; }
    
    public bool AlertasStockHabilitadas { get; set; }
    public bool NotificacionesStockBajo { get; set; }
    
    public DateTime FechaActualizacion { get; set; }
}

[AtLeastOneProperty(ErrorMessage = "Se debe proporcionar al menos un campo para actualizar.")]
public class KioscoConfigUpdateDto
{
    [StringLength(3, MinimumLength = 3, ErrorMessage = "La moneda debe tener exactamente 3 caracteres.")]
    public string? Moneda { get; set; }

    [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100.")]
    public decimal? ImpuestoPorcentaje { get; set; }

    [StringLength(10, MinimumLength = 3, ErrorMessage = "El prefijo SKU debe tener entre 3 y 10 caracteres.")]
    public string? PrefijoSku { get; set; }

    [Range(1, 1000, ErrorMessage = "El stock mínimo debe estar entre 1 y 1000.")]
    public int? StockMinimoDefault { get; set; }

    public bool? AlertasStockHabilitadas { get; set; }

    public bool? NotificacionesStockBajo { get; set; }
}
