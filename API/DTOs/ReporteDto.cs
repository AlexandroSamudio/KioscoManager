namespace API.DTOs;

public class ReporteDto
{
    public decimal TotalVentas { get; set; }

    public decimal CostoMercaderiaVendida { get; set; }

    public decimal GananciaBruta { get; set; }

    public int NumeroTransacciones { get; set; }

    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
}
