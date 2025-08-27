using FluentValidation;
using API.DTOs;

namespace API.Validators;

public class VentaDtoValidator : AbstractValidator<VentaDto>
{
    public VentaDtoValidator()
    {
        RuleFor(x => x.Fecha).RequiredField("fecha");
        RuleFor(x => x.Total).GreaterThanZero("total");
        RuleFor(x => x.CantidadProductos).GreaterThanZero("cantidadProductos");
    }
}

public class VentaCreateDtoValidator : AbstractValidator<VentaCreateDto>
{
    public VentaCreateDtoValidator()
    {
        RuleFor(x => x.Productos).RequiredField("productos");
        RuleForEach(x => x.Productos).SetValidator(new ProductoVentaDtoValidator());
    }
}

public class ProductoVentaDtoValidator : AbstractValidator<ProductoVentaDto>
{
    public ProductoVentaDtoValidator()
    {
        RuleFor(x => x.ProductoId).RequiredField("productoId");
        RuleFor(x => x.Cantidad).GreaterThanZero("cantidad");
    }
}
