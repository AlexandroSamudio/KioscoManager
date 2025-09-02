namespace API.DTOs
{
    public class CategoriaDto
    {
        public int Id { get; set; }
        public string? Nombre { get; set; }
    }

    public class CategoriaCreateDto
    {
        public string? Nombre { get; set; }
    }
    
    public class CategoriaUpdateDto
    {
        public string? Nombre { get; set; }
    }
    
    public class CategoriasRentabilidadDto
    {
        public int CategoriaId { get; set; }
        public string? Nombre { get; set; }
        public decimal PorcentajeVentas { get; set; }
    }
}
