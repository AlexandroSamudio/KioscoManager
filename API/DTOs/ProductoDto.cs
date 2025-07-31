using System.ComponentModel.DataAnnotations;
using API.Attributes;

namespace API.DTOs
{
    public class ProductoDto
    {
        public required int Id { get; set; }
        [Required, StringLength(13, MinimumLength = 13, ErrorMessage = "El SKU debe tener 13 caracteres.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El SKU debe contener exactamente 13 dígitos numéricos (EAN-13).")]
        public required string Sku { get; set; } = default!;
        [Required, StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
        public required string Nombre { get; set; } = default!;
        [StringLength(50, MinimumLength = 2, ErrorMessage = "La descripción debe tener entre 2 y 50 caracteres.")]
        public string? Descripcion { get; set; }
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public required decimal PrecioCompra { get; set; }
        [Range(0.01, 1000000)]
        [DataType(DataType.Currency)]
        public required decimal PrecioVenta { get; set; }
        [Range(0, 1000000)]
        public required int Stock { get; set; }
        [Required, StringLength(100)]
        public required string CategoriaNombre { get; set; } = default!;
    }

    [AtLeastOneProperty(ErrorMessage = "Se debe proporcionar al menos un campo para actualizar.")]
    public class ProductoUpdateDto
    {
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres.")]
        public string? Nombre { get; set; }

        [StringLength(13, MinimumLength = 13, ErrorMessage = "El SKU debe tener 13 caracteres.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El SKU debe contener exactamente 13 dígitos numéricos (EAN-13).")]
        public string? Sku { get; set; }
        
        [StringLength(500, ErrorMessage = "La descripción no puede tener más de 500 caracteres.")]
        public string? Descripcion { get; set; }
        
        [Range(0.01, 1000000, ErrorMessage = "El precio de compra debe estar entre 0.01 y 1,000,000.")]
        [DataType(DataType.Currency)]
        public decimal? PrecioCompra { get; set; }
        
        [Range(0.01, 1000000, ErrorMessage = "El precio de venta debe estar entre 0.01 y 1,000,000.")]
        [DataType(DataType.Currency)]
        public decimal? PrecioVenta { get; set; }
        
        [Range(0, 1000000, ErrorMessage = "El stock debe estar entre 0 y 1,000,000.")]
        public int? Stock { get; set; }
        
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public int? CategoriaId { get; set; }
    }

    public class ProductoCreateDto
    {
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre del producto debe tener entre 2 y 50 caracteres.")]
        public required string Nombre { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una categoría válida.")]
        public required int CategoriaId { get; set; }

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "El precio de compra debe estar entre 0.01 y 1,000,000.")]
        [DataType(DataType.Currency)]
        public required decimal PrecioCompra { get; set; }

        [Required]
        [Range(0.01, 1000000, ErrorMessage = "El precio de venta debe estar entre 0.01 y 1,000,000.")]
        [DataType(DataType.Currency)]
        public required decimal PrecioVenta { get; set; }

        [Required]
        [Range(0, 1000000, ErrorMessage = "El stock debe estar entre 0 y 1,000,000.")]
        public required int Stock { get; set; }

        [StringLength(500, ErrorMessage = "La descripción no puede exceder 500 caracteres.")]
        public string? Descripcion { get; set; }

        [Required]
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El SKU debe tener 13 caracteres.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El SKU debe contener exactamente 13 dígitos numéricos (EAN-13).")]
        public required string Sku { get; set; }
    }

    public class ProductoMasVendidoDto
    {
        [Required]
        public required int ProductoId { get; set; }
        
        [Required]
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre del producto debe tener entre 2 y 50 caracteres.")]
        public required string NombreProducto { get; set; }
            
        [StringLength(13, MinimumLength = 13, ErrorMessage = "El SKU debe tener 13 caracteres.")]
        [RegularExpression(@"^\d{13}$", ErrorMessage = "El SKU debe contener exactamente 13 dígitos numéricos (EAN-13).")]
        public string? Sku { get; set; }

        [Required]
        public required int CantidadVendida { get; set; }
        
        public decimal TotalVentas { get; set; }
        
        [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre de la categoría debe tener entre 2 y 50 caracteres.")]
        public string? CategoriaNombre { get; set; }
    }
}