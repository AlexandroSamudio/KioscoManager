using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class KioscoConfigDto
{
    public int Id { get; set; }
    public int KioscoId { get; set; }
    
    [Required]
    [MaxLength(3)]
    public required string Moneda { get; set; }
    
    [Range(0, 100)]
    public decimal ImpuestoPorcentaje { get; set; }
    
    [MaxLength(10)]
    public string? PrefijoSku { get; set; }
    
    [Range(1, 1000)]
    public int StockMinimoDefault { get; set; }
    
    public bool AlertasStockHabilitadas { get; set; }
    public bool NotificacionesStockBajo { get; set; }
    
    public DateTime FechaActualizacion { get; set; }
}

public class KioscoConfigUpdateDto
{
    [MaxLength(3,ErrorMessage = "La moneda debe tener un máximo de 3 caracteres.")]
    public string? Moneda { get; set; }
    
    [Range(0, 100, ErrorMessage = "El impuesto debe estar entre 0 y 100.")]
    public decimal? ImpuestoPorcentaje { get; set; }
    
    [MaxLength(10, ErrorMessage = "El prefijo SKU debe tener un máximo de 10 caracteres.")]
    public string? PrefijoSku { get; set; }
    
    [Range(1, 1000, ErrorMessage = "El stock mínimo debe estar entre 1 y 1000.")]
    public int? StockMinimoDefault { get; set; }
    
    public bool? AlertasStockHabilitadas { get; set; }

    public bool? NotificacionesStockBajo { get; set; }
}
