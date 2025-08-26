using FluentValidation;
using API.DTOs;

namespace API.Validators;

public class CompraDetalleDtoValidator : AbstractValidator<CompraDetalleDto>
{
  public CompraDetalleDtoValidator()
  {
    RuleFor(x => x.ProductoId).GreaterThanZero("productoId");
    RuleFor(x => x.Cantidad).GreaterThanZero("cantidad");
    RuleFor(x => x.CostoUnitario).GreaterThanZero("costoUnitario");
  }
}

public class CompraCreateDtoValidator : AbstractValidator<CompraCreateDto>
{
  public CompraCreateDtoValidator()
  {
    RuleForEach(x => x.Detalles).SetValidator(new CompraDetalleDtoValidator());
    RuleFor(x => x.Detalles).RequiredField("detalles");
    RuleFor(x => x.Proveedor!).NameRules();
    RuleFor(x => x.Nota!).NameRules();
  }
}

public class CompraDetalleViewDtoValidator : AbstractValidator<CompraDetalleViewDto>
{
  public CompraDetalleViewDtoValidator()
  {
    RuleFor(x => x.ProductoId).GreaterThanZero("productoId");

    RuleFor(x => x.ProductoNombre!).NameRules();

    RuleFor(x => x.ProductoSku!).NameRules();

    RuleFor(x => x.Cantidad).GreaterThanZero("cantidad");

    RuleFor(x => x.CostoUnitario).GreaterThanZero("costoUnitario");

    RuleFor(x => x.Subtotal).GreaterThanZero("subtotal");
  }
}

public class CompraDtoValidator : AbstractValidator<CompraDto>
{
  public CompraDtoValidator()
  {
    RuleFor(x => x.Fecha).RequiredField("fecha");

    RuleFor(x => x.CostoTotal).GreaterThanZero("costoTotal");

    RuleFor(x => x.Proveedor!).NameRules();

    RuleFor(x => x.Nota!).NameRules();

    RuleFor(x => x.UsuarioId).RequiredField("usuarioId");

    RuleFor(x => x.Detalles).RequiredField("detalles");

    RuleForEach(x => x.Detalles).SetValidator(new CompraDetalleViewDtoValidator());
  }
}