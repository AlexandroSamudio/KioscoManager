using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        
        [Required]
        [MaxLength(100)]
        public required string Nombre { get; set; }
    }

    public class CategoriaCreateDto
    {
        [Required]
        [MaxLength(100,ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public required string Nombre { get; set; }
    }

    public class CategoriaUpdateDto
    {
        [MaxLength(100,ErrorMessage = "El nombre no puede exceder 100 caracteres")]
        [MinLength(2, ErrorMessage = "El nombre debe tener al menos 2 caracteres")]
        public string? Nombre { get; set; }
    }
}
