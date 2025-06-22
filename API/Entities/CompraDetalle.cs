using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class CompraDetalle
{
    public int Id { get; set; }
    public required int CompraId { get; set; }
    public Compra? Compra { get; set; }
    public required int ProductoId { get; set; }
    public Producto? Producto { get; set; }
    [Range(1, int.MaxValue, ErrorMessage = "La cantidad debe ser mayor que cero")]
    public required int Cantidad { get; set; }
    [Column(TypeName = "decimal(18, 2)")]
    [Range(0.01, double.MaxValue, ErrorMessage = "El costo unitario debe ser mayor que cero")]
    public required decimal CostoUnitario { get; set; }
}
