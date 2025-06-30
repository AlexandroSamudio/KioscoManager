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
    [Required]
    [MaxLength(3)]
    public required string Moneda { get; set; }
    
    [Range(0, 100)]
    public decimal ImpuestoPorcentaje { get; set; }
    
    [Range(0, 4)]
    public int DecimalesPrecios { get; set; }
    
    [MaxLength(10)]
    public string? PrefijoSku { get; set; }
    
    [Range(1, 1000)]
    public int StockMinimoDefault { get; set; }
    
    public bool AlertasStockHabilitadas { get; set; }

    public bool NotificacionesStockBajo { get; set; }
}
