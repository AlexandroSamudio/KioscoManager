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
}

public sealed class ProductoDtoValidator : AbstractValidator<ProductoDto>
{
    public ProductoDtoValidator()
    {
        RuleFor(p => p.Nombre).RequiredField("nombre").NameRules(2, 50);
        RuleFor(p => p.Sku)
            .RequiredField("sku")
            .Must(ProductoValidationHelpers.Ean13IsValid).WithMessage("El SKU debe ser un código EAN-13 válido.");
        RuleFor(p => p.Descripcion).NameRules(2, 100);
        RuleFor(p => p.PrecioCompra).GreaterThanZero("precioCompra");
        RuleFor(p => p.PrecioVenta)
            .GreaterThanZero("precioVenta")
            .GreaterThanOrEqualTo(p => p.PrecioCompra).WithMessage("precioVenta debe ser mayor a precioCompra");
        RuleFor(p => p.Stock).GreaterThanZero("stock");
        RuleFor(p => p.CategoriaNombre).NameRules(2, 50);
        RuleFor(p => p.CategoriaId).GreaterThanZero("categoriaId");
    }
}

public sealed class ProductoUpdateDtoValidator : AbstractValidator<ProductoUpdateDto>
{
    public ProductoUpdateDtoValidator()
    {
        RuleFor(p => p)
            .AtLeastOneProperty("Se debe proporcionar al menos un campo para actualizar.");

        RuleFor(p => p.Nombre).NameRules(2, 50);
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
            .GreaterThanOrEqualTo(p => p.PrecioCompra).WithMessage("precioVenta debe ser mayor a precioCompra");
        RuleFor(p => p.Stock)
            .GreaterThanZero("stock")
            .When(p => p.Stock.HasValue);
        RuleFor(p => p.CategoriaId)
            .GreaterThanZero("categoriaId")
            .When(p => p.CategoriaId.HasValue);
    }
}

public sealed class ProductoCreateDtoValidator : AbstractValidator<ProductoCreateDto>
{
    public ProductoCreateDtoValidator()
    {
        RuleFor(p => p.Nombre).RequiredField("nombre").NameRules(2, 50);
        RuleFor(p => p.Sku)
            .RequiredField("sku")
            .Must(ProductoValidationHelpers.Ean13IsValid).WithMessage("El SKU debe ser un código EAN-13 válido.");
        RuleFor(p => p.Descripcion).NameRules(2, 100);
        RuleFor(p => p.PrecioCompra).GreaterThanZero("precioCompra");
        RuleFor(p => p.PrecioVenta).GreaterThanZero("precioVenta")
            .GreaterThanOrEqualTo(p => p.PrecioCompra).WithMessage("precioVenta debe ser mayor a precioCompra");
        RuleFor(p => p.Stock).GreaterThanZero("stock");
        RuleFor(p => p.CategoriaId).GreaterThanZero("categoriaId");
    }
}
