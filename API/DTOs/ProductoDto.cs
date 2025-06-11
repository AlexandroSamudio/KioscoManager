namespace API.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public required string Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal Precio { get; set; }
        public int Stock { get; set; }
        public required string CategoriaNombre { get; set; }
    }
}
