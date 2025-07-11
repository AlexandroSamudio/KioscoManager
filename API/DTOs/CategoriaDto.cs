using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        
        [Required]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public required string Nombre { get; set; }
    }

    public class CategoriaCreateDto
    {
        [Required(ErrorMessage = "El nombre es requerido.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public required string Nombre { get; set; }
    }

    [AtLeastOneProperty(ErrorMessage = "Se debe proporcionar al menos un campo para actualizar.")]
    public class CategoriaUpdateDto
    {
        [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres.")]
        public string? Nombre { get; set; }
    }
}
