using API.DTOs;
using FluentValidation;

namespace API.Validators;

public class CategoriaDtoValidator : AbstractValidator<CategoriaDto>
{
    public CategoriaDtoValidator()
    {
        RuleFor(c => c.Nombre).RequiredField("nombre").NameRules(2, 50);
    }
}

public class CategoriaCreateDtoValidator : AbstractValidator<CategoriaCreateDto>
{
    public CategoriaCreateDtoValidator()
    {
        RuleFor(c => c.Nombre).RequiredField("nombre").NameRules(2, 50);
    }
}

public class CategoriaUpdateDtoValidator : AbstractValidator<CategoriaUpdateDto>
{
    public CategoriaUpdateDtoValidator()
    {
        RuleFor(c => c.Nombre).RequiredField("nombre").NameRules(2, 50);
    }
}
