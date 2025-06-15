

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Producto
{
    public int Id { get; set; }
    [MaxLength(50)]
    public required string Sku { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrecioCompra { get; set; }
    [Required]
    [Column(TypeName = "decimal(18, 2)")]
    public decimal PrecioVenta { get; set; }

    public int Stock { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int KioscoId { get; set; }   
    public Kiosco? Kiosco { get; set; }
}

