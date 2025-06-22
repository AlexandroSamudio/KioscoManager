using System.ComponentModel.DataAnnotations;

namespace API.Entities;

public class Kiosco
{
    public int Id { get; set; }

    [MaxLength(100)]
    public required string Nombre { get; set; }
    public ICollection<AppUser> Usuarios { get; set; } = new List<AppUser>();
    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
    public ICollection<Venta> Ventas { get; set; } = new List<Venta>();
    public ICollection<Compra> Compras { get; set; } = new List<Compra>();
    public ICollection<CodigoInvitacion> CodigosInvitacion { get; set; } = new List<CodigoInvitacion>();


}
