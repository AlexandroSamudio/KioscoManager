using System.ComponentModel.DataAnnotations.Schema;

namespace API.DTOs
{
    public class ProductoInfoDto
    {
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalCapitalInvertido { get; set; }
    }
}
