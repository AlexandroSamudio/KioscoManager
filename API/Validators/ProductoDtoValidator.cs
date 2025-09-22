using API.DTOs;
using FluentValidation;

namespace API.Validators;

internal static class ProductoValidationHelpers
{
    public static bool Ean13IsValid(string? sku)
    {
        if (string.IsNullOrWhiteSpace(sku))
            return false;

        if (sku.Length != 13)
            return false;

        if (!sku.All(char.IsDigit))
            return false;

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = int.Parse(sku[i].ToString());
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;
        int providedCheckDigit = int.Parse(sku[12].ToString());

        return checkDigit == providedCheckDigit;
    }

    public static bool BeValidHttpUrl(string? url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return true; 

        return Uri.TryCreate(url, UriKind.Absolute, out Uri? result)
            && (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    public static bool BeValidImageFile(IFormFile? file)
    {
        if (file == null)
            return true;

        
        const long maxFileSize = 5 * 1024 * 1024;
        if (file.Length > maxFileSize)
            return false;

        
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var allowedContentTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };

        var fileExtension = Path.GetExtension(file.FileName)?.ToLowerInvariant();
        
        return !string.IsNullOrEmpty(fileExtension) 
            && allowedExtensions.Contains(fileExtension)
            && allowedContentTypes.Contains(file.ContentType?.ToLowerInvariant());
    }
}

public sealed class ProductoDtoValidator : AbstractValidator<ProductoDto>
{
    public ProductoDtoValidator()
    {
        RuleFor(p => p.Nombre).RequiredField("nombre").NameRules();
        RuleFor(p => p.Sku)
            .RequiredField("sku")
            .Must(ProductoValidationHelpers.Ean13IsValid).WithMessage("El SKU debe ser un código EAN-13 válido.");
        RuleFor(p => p.Descripcion).NameRules(2, 100);
        RuleFor(p => p.PrecioCompra).GreaterThanZero("precioCompra");
        RuleFor(p => p.PrecioVenta)
            .GreaterThanZero("precioVenta")
            .GreaterThan(p => p.PrecioCompra).WithMessage("precioVenta debe ser mayor a precioCompra");
        RuleFor(p => p.Stock).GreaterThanZero("stock");
        RuleFor(p => p.CategoriaNombre).NameRules();
        RuleFor(p => p.CategoriaId).GreaterThanZero("categoriaId");
        RuleFor(p => p.ImageUrl)
            .MaximumLength(2048).WithMessage("La URL de la imagen no puede exceder los 2048 caracteres.")
            .Must(ProductoValidationHelpers.BeValidHttpUrl).WithMessage("La URL de la imagen debe ser una URL HTTP o HTTPS válida.")
            .When(p => !string.IsNullOrWhiteSpace(p.ImageUrl));
    }
}

public sealed class ProductoUpdateDtoValidator : AbstractValidator<ProductoUpdateDto>
{
    public ProductoUpdateDtoValidator()
    {
        RuleFor(p => p)
            .AtLeastOneProperty("Se debe proporcionar al menos un campo para actualizar.");

        RuleFor(p => p.Nombre)
            .NameRules()
            .When(p => !string.IsNullOrWhiteSpace(p.Nombre));
        RuleFor(p => p.Sku)
            .Must(ProductoValidationHelpers.Ean13IsValid).WithMessage("El SKU debe ser un código EAN-13 válido.")
            .When(p => !string.IsNullOrWhiteSpace(p.Sku));
        RuleFor(p => p.Descripcion).NameRules(2, 100);
        RuleFor(p => p.PrecioCompra)
            .GreaterThanZero("precioCompra")
            .When(p => p.PrecioCompra.HasValue);
        RuleFor(p => p.PrecioVenta)
           .GreaterThanZero("precioVenta")
           .When(p => p.PrecioVenta.HasValue)
           .GreaterThan(p => p.PrecioCompra!.Value)
           .WithMessage("precioVenta debe ser mayor que precioCompra")
           .When(p => p.PrecioVenta.HasValue && p.PrecioCompra.HasValue);
        RuleFor(p => p.Stock)
            .GreaterThanZero("stock")
            .When(p => p.Stock.HasValue);
        RuleFor(p => p.CategoriaId)
            .GreaterThanZero("categoriaId")
            .When(p => p.CategoriaId.HasValue);
        RuleFor(p => p.ImageFile)
            .Must(ProductoValidationHelpers.BeValidImageFile).WithMessage("El archivo de imagen debe ser JPG, JPEG, PNG o WebP y no exceder los 5MB.")
            .When(p => p.ImageFile != null);
    }
}

public sealed class ProductoCreateDtoValidator : AbstractValidator<ProductoCreateDto>
{
    public ProductoCreateDtoValidator()
    {
        RuleFor(p => p.Nombre).RequiredField("nombre").NameRules();
        RuleFor(p => p.Sku)
            .RequiredField("sku")
            .Must(ProductoValidationHelpers.Ean13IsValid).WithMessage("El SKU debe ser un código EAN-13 válido.");
        RuleFor(p => p.Descripcion).NameRules(2, 100);
        RuleFor(p => p.PrecioCompra).GreaterThanZero("precioCompra");
        RuleFor(p => p.PrecioVenta).GreaterThanZero("precioVenta")
            .GreaterThan(p => p.PrecioCompra).WithMessage("precioVenta debe ser mayor a precioCompra");
        RuleFor(p => p.Stock).GreaterThanZero("stock");
        RuleFor(p => p.CategoriaId).GreaterThanZero("categoriaId");
        RuleFor(p => p.ImageFile)
            .Must(ProductoValidationHelpers.BeValidImageFile).WithMessage("El archivo de imagen debe ser JPG, JPEG, PNG o WebP y no exceder los 5MB.")
            .When(p => p.ImageFile != null);
    }
}
