using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class VentaCreateDto
{
    [Required]
    public required List<ProductoVentaDto> Productos { get; set; }
}

public class ProductoVentaDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "El ProductoId debe ser mayor que cero")]
    public required int ProductoId { get; set; }
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
    public required int Cantidad { get; set; }
}
