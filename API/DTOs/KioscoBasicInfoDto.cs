using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs;

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
