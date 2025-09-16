using API.DTOs;
using FluentValidation;

namespace API.Validators;

public sealed class UserDtoValidator : AbstractValidator<UserDto>
{
    public UserDtoValidator()
    {
        RuleFor(x => x.Email).RequiredField("email").EmailAddress().WithMessage("El email no es válido.");
        RuleFor(x => x.Token).RequiredField("token");
        RuleFor(x => x.UserName).RequiredField("userName").NameRules();
    }
}

public sealed class UserManagementDtoValidator : AbstractValidator<UserManagementDto>
{
    public UserManagementDtoValidator()
    {
        RuleFor(x => x.Email).RequiredField("email").EmailAddress().WithMessage("El email no es válido.");
        RuleFor(x => x.UserName).RequiredField("userName").NameRules();
    }
}

public sealed class ProfileUpdateDtoValidator : AbstractValidator<ProfileUpdateDto>
{
    public ProfileUpdateDtoValidator()
    {
        RuleFor(p => p)
            .AtLeastOneProperty("Se debe proporcionar al menos un campo para actualizar.");

        RuleFor(x => x.UserName)
            .NameRules()
            .When(x => !string.IsNullOrWhiteSpace(x.UserName));

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("El email no es válido.")
            .When(x => !string.IsNullOrWhiteSpace(x.Email));
    }
}

public sealed class ChangePasswordDtoValidator : AbstractValidator<ChangePasswordDto>
{
    public ChangePasswordDtoValidator()
    {
        RuleFor(x => x.CurrentPassword).RequiredField("currentPassword");

        RuleFor(x => x.NewPassword)
            .RequiredField("newPassword")
            .Matches(@"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])(?=.*[\W_]).{8,128}$")
            .WithMessage("La contraseña debe contener al menos una letra mayúscula, una letra minúscula, un número, un carácter especial y tener entre 8 y 128 caracteres.");

        RuleFor(x => x.ConfirmPassword)
            .RequiredField("confirmPassword")
            .Equal(x => x.NewPassword).WithMessage("Las contraseñas no coinciden");
    }
}
