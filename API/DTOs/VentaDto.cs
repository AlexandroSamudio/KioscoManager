namespace API.DTOs
{
    public class VentaDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
        public int CantidadProductos { get; set; }
    }

    public class VentaCreateDto
    {
        public List<ProductoVentaDto> Productos { get; set; } = [];
    }

    public class ProductoVentaDto
    {
        public int ProductoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class VentasPorDiaDto
    {
        public DateTime Fecha { get; set; }

        public decimal TotalVentas { get; set; }

        /// Tipo de agrupación utilizada: "daily", "weekly", "monthly"
        public string? TipoAgrupacion { get; set; }
    }

    public class VentaChartDto
    {
        public int Id { get; set; }
        public DateTime Fecha { get; set; }
        public decimal Total { get; set; }
    }
}
