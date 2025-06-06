

namespace API.Entities;

public class Producto
{
    public int Id { get; set; }
    public required string Nombre { get; set; }
    public string? Descripcion { get; set; }
    public decimal Precio { get; set; }
    public int Stock { get; set; }

    public int CategoriaId { get; set; }
    public Categoria? Categoria { get; set; }
    public int KioscoId { get; set; }   
    public Kiosco? Kiosco { get; set; }
}

