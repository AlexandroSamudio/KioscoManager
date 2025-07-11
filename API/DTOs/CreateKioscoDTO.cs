using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateKioscoDto
{
    [Required]
    [StringLength(50, MinimumLength = 3, ErrorMessage = "El nombre del kiosco debe tener entre 3 y 50 caracteres.")]
    public required string Nombre { get; set; }
}
