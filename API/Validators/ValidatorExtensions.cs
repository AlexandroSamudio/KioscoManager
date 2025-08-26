using FluentValidation;

namespace API.Validators;

public static class ValidatorExtensions
{
  public static IRuleBuilderOptions<T, string> NameRules<T>(this IRuleBuilder<T, string> ruleBuilder, int min = 2, int max = 50)
  {
    return ruleBuilder
      .MinimumLength(min).WithMessage($"El campo debe tener al menos {min} caracteres.")
      .MaximumLength(max).WithMessage($"El campo no puede exceder los {max} caracteres.");
  }

  public static IRuleBuilderOptions<T, TProperty> RequiredField<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string fieldName)
  {
    return ruleBuilder
      .NotEmpty().WithMessage($"El campo {fieldName} es requerido.");
  }
  public static IRuleBuilderOptions<T, TProperty> GreaterThanZero<T, TProperty>(this IRuleBuilder<T, TProperty> ruleBuilder, string fieldName)
    where TProperty : struct, IComparable<TProperty>
  {
    return ruleBuilder.Must(value =>
    {
      return value switch
      {
        int intValue => intValue > 0,
        decimal decValue => decValue > 0,
        float floatValue => floatValue > 0,
        double doubleValue => doubleValue > 0,
        long longValue => longValue > 0,
        short shortValue => shortValue > 0,
        byte byteValue => byteValue > 0,
        _ => Convert.ToDouble(value) > 0
      };
    }).WithMessage($"El campo {fieldName} debe ser mayor que cero.");
  }
}