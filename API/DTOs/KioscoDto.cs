namespace API.DTOs;

public class CreateKioscoDto
{
    public string? Nombre { get; set; }
}

public class JoinKioscoDto
{
    public string? CodigoInvitacion { get; set; }
}

public class KioscoBasicInfoDto
{
    public int Id { get; set; }
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}
public class KioscoBasicInfoUpdateDto
{
    public string? Nombre { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
}

public class KioscoConfigDto
{
    public int Id { get; set; }
    public int KioscoId { get; set; }
    public string? Moneda { get; set; }
    public decimal ImpuestoPorcentaje { get; set; }
    public string? PrefijoSku { get; set; }
    public int StockMinimoDefault { get; set; }
    public bool AlertasStockHabilitadas { get; set; }
    public bool NotificacionesStockBajo { get; set; }
    public DateTime FechaActualizacion { get; set; }
}
public class KioscoConfigUpdateDto
{
    public string? Moneda { get; set; }
    public decimal? ImpuestoPorcentaje { get; set; }
    public string? PrefijoSku { get; set; }
    public int? StockMinimoDefault { get; set; }
    public bool? AlertasStockHabilitadas { get; set; }
    public bool? NotificacionesStockBajo { get; set; }
}
