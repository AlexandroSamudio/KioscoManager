using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class KioscoConfig
{
    public int Id { get; set; }
    
    public int KioscoId { get; set; }
    public Kiosco Kiosco { get; set; } = null!;
    
    [MaxLength(3)]
    public string Moneda { get; set; } = "ARS";
    
    public decimal ImpuestoPorcentaje { get; set; } = 21.0m;
    
    public int DecimalesPrecios { get; set; } = 2;
    
    [MaxLength(10)]
    public string? PrefijoSku { get; set; }
    
    public int StockMinimoDefault { get; set; } = 5;
    
    public bool AlertasStockHabilitadas { get; set; } = true;
    
    public DateTime FechaCreacion { get; set; } = DateTime.UtcNow;
    
    public DateTime FechaActualizacion { get; set; } = DateTime.UtcNow;

    public static KioscoConfig CreateDefault(int kioscoId)
    {
        return new KioscoConfig
        {
            KioscoId = kioscoId,
            Moneda = "ARS",
            ImpuestoPorcentaje = 21.0m,
            DecimalesPrecios = 2,
            StockMinimoDefault = 5,
            AlertasStockHabilitadas = true,
            FechaCreacion = DateTime.UtcNow,
            FechaActualizacion = DateTime.UtcNow
        };
    }
}
