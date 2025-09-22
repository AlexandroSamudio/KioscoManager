using Microsoft.AspNetCore.Http;

namespace API.DTOs
{
    public class ProductoDto
    {
        public int Id { get; set; }
        public string? Sku { get; set; }
        public string? Nombre { get; set; }
        public string? Descripcion { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public string? CategoriaNombre { get; set; }
        public int CategoriaId { get; set; }
        public string? ImageUrl { get; set; }
    }
    
    public class ProductoUpdateDto
    {
        public string? Nombre { get; set; }
        public string? Sku { get; set; }
        public string? Descripcion { get; set; }
        public decimal? PrecioCompra { get; set; }
        public decimal? PrecioVenta { get; set; }
        public int? Stock { get; set; }
        public int? CategoriaId { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class ProductoCreateDto
    {
        public string? Nombre { get; set; }
        public int CategoriaId { get; set; }
        public decimal PrecioCompra { get; set; }
        public decimal PrecioVenta { get; set; }
        public int Stock { get; set; }
        public string? Descripcion { get; set; }
        public string? Sku { get; set; }
        public IFormFile? ImageFile { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        public int ProductoId { get; set; }
        public string? NombreProducto { get; set; }
        public string? Sku { get; set; }
        public int CantidadVendida { get; set; }
        public decimal TotalVentas { get; set; }
        public string? CategoriaNombre { get; set; }
    }
}