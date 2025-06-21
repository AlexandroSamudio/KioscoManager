using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class VentaCreateDto
{
    public required List<ProductoVentaDto> Productos { get; set; }
}

public class ProductoVentaDto
{
    [Required]
    public required int ProductoId { get; set; }
    [Required]
    public required int Cantidad { get; set; }
}
