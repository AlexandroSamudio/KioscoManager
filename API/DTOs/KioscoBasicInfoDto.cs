using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class KioscoBasicInfoDto
{
    public int Id { get; set; }
    
    [Required]
    public required string Nombre { get; set; }
    
    public string? Direccion { get; set; }
    
    public string? Telefono { get; set; }
}

public class KioscoBasicInfoUpdateDto
{
    [MaxLength(100, ErrorMessage = "El nombre no puede exceder 100 caracteres")]
    [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
    public string? Nombre { get; set; }
    
    [MaxLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    public string? Direccion { get; set; }
    
    [MaxLength(20, ErrorMessage = "El teléfono no puede exceder 20 caracteres")]
    [RegularExpression(@"^[\d\s\-\+\(\)]*$", ErrorMessage = "El teléfono contiene caracteres no válidos")]
    public string? Telefono { get; set; }
}
