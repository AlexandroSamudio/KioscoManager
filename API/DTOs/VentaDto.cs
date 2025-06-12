namespace API.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public string? NombreUsuario { get; set; }
        public int CantidadProductos { get; set; }
    }
}
