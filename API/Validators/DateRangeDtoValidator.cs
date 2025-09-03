using API.DTOs;
using FluentValidation;

namespace API.Validators;

public sealed class DateRangeDtoValidator : AbstractValidator<DateRangeDto>
{
    
    public DateRangeDtoValidator()
    {
        RuleFor(x => x.FechaInicio)
            .NotEmpty().WithMessage("La fecha de inicio es requerida.")
            .LessThan(x => x.FechaFin).WithMessage("La fecha de inicio debe ser anterior a la fecha de fin.");

        RuleFor(x => x.FechaFin)
            .NotEmpty().WithMessage("La fecha de fin es requerida.");
    }

}
