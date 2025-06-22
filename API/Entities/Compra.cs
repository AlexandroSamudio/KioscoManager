using System.ComponentModel.DataAnnotations.Schema;

namespace API.Entities;

public class Compra
{
    public int Id { get; set; }
    public required DateTime Fecha { get; set; } = DateTime.UtcNow;

    [Column(TypeName = "decimal(18, 2)")]
    public required decimal CostoTotal { get; set; }
    public string? Proveedor { get; set; }
    public string? Nota { get; set; }
    public required int KioscoId { get; set; }
    public Kiosco? Kiosco { get; set; }
    public required int UsuarioId { get; set; }
    public AppUser? Usuario { get; set; }
    public required ICollection<CompraDetalle> Detalles { get; set; } = new List<CompraDetalle>();
}
