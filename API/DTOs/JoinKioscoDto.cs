using System.ComponentModel.DataAnnotations;

namespace API.DTOs
{
    public class JoinKioscoDto
    {
        [Required]
        public required string CodigoInvitacion { get; set; }
    }
}
