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
        [MaxLength(100)]
        [MinLength(2)]
        public required string Nombre { get; set; }
    }

    public class CategoriaUpdateDto
    {
        [Required]
        [MaxLength(100)]
        [MinLength(2)]
        public required string Nombre { get; set; }
    }
}
