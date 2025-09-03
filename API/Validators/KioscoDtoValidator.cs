using API.DTOs;
using FluentValidation;

namespace API.Validators;

public sealed class CreateKioscoDtoValidator : AbstractValidator<CreateKioscoDto>
{
    public CreateKioscoDtoValidator()
    {
        RuleFor(x => x.Nombre).RequiredField("nombre").NameRules();
    }
}

public sealed class JoinKioscoDtoValidator : AbstractValidator<JoinKioscoDto>
{
    public JoinKioscoDtoValidator()
    {
        RuleFor(x => x.CodigoInvitacion).RequiredField("codigoInvitacion").NameRules(10, 10);
    }
}

public sealed class KioscoBasicInfoDtoValidator : AbstractValidator<KioscoBasicInfoDto>
{
    public KioscoBasicInfoDtoValidator()
    {
        RuleFor(x => x.Nombre).RequiredField("nombre").NameRules();

        RuleFor(x => x.Direccion).RequiredField("direccion").NameRules();

        RuleFor(x => x.Telefono)
            .RequiredField("telefono")
            .NameRules(10, 10)
            .Matches(@"^\d+$").WithMessage("El teléfono debe contener solo números.");
    }
}

public sealed class KioscoBasicInfoUpdateDtoValidator : AbstractValidator<KioscoBasicInfoUpdateDto>
{
    public KioscoBasicInfoUpdateDtoValidator()
    {
        RuleFor(p => p)
            .AtLeastOneProperty("Se debe proporcionar al menos un campo para actualizar.");

        RuleFor(x => x.Nombre)
            .NameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.Nombre));

        RuleFor(x => x.Direccion)
            .NameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.Direccion));

        RuleFor(x => x.Telefono)
            .NameRules(10, 10)
            .Matches(@"^\d+$").WithMessage("El teléfono debe contener solo números.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefono));
    }
}

public sealed class KioscoConfigDtoValidator : AbstractValidator<KioscoConfigDto>
{
    public KioscoConfigDtoValidator()
    {
        RuleFor(x => x.Moneda).RequiredField("moneda").Length(3).WithMessage("La moneda debe tener exactamente 3 caracteres.");

        RuleFor(x => x.ImpuestoPorcentaje)
            .InclusiveBetween(0, 100)
            .WithMessage("El impuesto debe estar entre 0 y 100.");

        RuleFor(x => x.PrefijoSku)
            .NameRules(3, 10)
            .When(x => !string.IsNullOrWhiteSpace(x.PrefijoSku));

        RuleFor(x => x.StockMinimoDefault)
            .InclusiveBetween(1, 1000)
            .WithMessage("El stock mínimo por defecto debe estar entre 1 y 1000.");
    }
}

public sealed class KioscoConfigUpdateDtoValidator : AbstractValidator<KioscoConfigUpdateDto>
{
    public KioscoConfigUpdateDtoValidator()
    {
        RuleFor(p => p)
            .AtLeastOneProperty("Se debe proporcionar al menos un campo para actualizar.");

        RuleFor(x => x.Moneda)
            .Length(3).WithMessage("La moneda debe tener exactamente 3 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Moneda));

        RuleFor(x => x.ImpuestoPorcentaje)
            .InclusiveBetween(0, 100)
            .WithMessage("El impuesto debe estar entre 0 y 100.")
            .When(x => x.ImpuestoPorcentaje.HasValue);

        RuleFor(x => x.PrefijoSku)
            .NameRules(3, 10)
            .When(x => !string.IsNullOrWhiteSpace(x.PrefijoSku));

        RuleFor(x => x.StockMinimoDefault)
            .InclusiveBetween(1, 1000)
            .WithMessage("El stock mínimo por defecto debe estar entre 1 y 1000.")
            .When(x => x.StockMinimoDefault.HasValue);
    }
}