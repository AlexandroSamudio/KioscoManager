using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs;

public class CreateKioscoDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del kiosco debe tener entre 3 y 50 caracteres.")]
    public required string Nombre { get; set; }
}

public class JoinKioscoDto
    {
        [Required]
        public required string CodigoInvitacion { get; set; }
    }

public class KioscoBasicInfoDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    public required string Nombre { get; set; }

    [StringLength(50, ErrorMessage = "La dirección no puede exceder 50 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres.")]
    public string? Telefono { get; set; }
}

[AtLeastOneProperty(ErrorMessage = "Se debe proporcionar al menos un campo para actualizar.")]
public class KioscoBasicInfoUpdateDto
{
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre debe tener entre 3 y 50 caracteres.")]
    public string? Nombre { get; set; }

    [StringLength(50, MinimumLength = 3, ErrorMessage = "La dirección debe tener entre 3 y 50 caracteres.")]
    public string? Direccion { get; set; }

    [StringLength(20, MinimumLength = 3, ErrorMessage = "El teléfono debe tener entre 3 y 20 caracteres.")]
    [RegularExpression(@"^[\d\s\-\+\(\)]*$", ErrorMessage = "El teléfono contiene caracteres no válidos")]
    public string? Telefono { get; set; }
}

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
