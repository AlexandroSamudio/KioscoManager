using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateKioscoDto
{
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public required string Nombre { get; set; }
}
