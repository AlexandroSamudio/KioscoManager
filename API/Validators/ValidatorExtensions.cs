using FluentValidation;

namespace API.Validators;

public static class ValidatorExtensions
{
  public static IRuleBuilderOptions<T, string?> NameRules<T>(this IRuleBuilder<T, string?> ruleBuilder, int min = 2, int max = 50)
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

  public static IRuleBuilderOptions<T, TProperty?> GreaterThanZero<T, TProperty>(this IRuleBuilder<T, TProperty?> ruleBuilder, string fieldName)
    where TProperty : struct, IComparable<TProperty>
  {
    return ruleBuilder.Must(value =>
    {
      if (!value.HasValue) return true;
      
      return value.Value switch
      {
        int intValue => intValue > 0,
        decimal decValue => decValue > 0,
        float floatValue => floatValue > 0,
        double doubleValue => doubleValue > 0,
        long longValue => longValue > 0,
        short shortValue => shortValue > 0,
        byte byteValue => byteValue > 0,
        _ => Convert.ToDouble(value.Value) > 0
      };
    }).WithMessage($"El campo {fieldName} debe ser mayor que cero.");
  }

  public static IRuleBuilderOptionsConditions<T, T> AtLeastOneProperty<T>(this IRuleBuilder<T, T> ruleBuilder, string message = "Se debe proporcionar al menos un campo.")
  {
    return ruleBuilder.Custom((obj, context) =>
    {
      if (obj == null)
      {
        context.AddFailure(message);
        return;
      }

      var type = obj.GetType();
      var properties = type.GetProperties();
      bool hasValue = false;

      foreach (var prop in properties)
      {
        var value = prop.GetValue(obj);
        
        switch (value)
        {
          case string str when !string.IsNullOrWhiteSpace(str):
          case int intVal when intVal > 0:
          case decimal decVal when decVal > 0:
          case double doubleVal when doubleVal > 0:
          case float floatVal when floatVal > 0:
          case long longVal when longVal > 0:
          case bool boolVal when boolVal:
            hasValue = true;
            break;
          case null:
            continue;
          default:
            if (value != null)
            {
              var valueType = value.GetType();
              if (valueType.IsGenericType && valueType.GetGenericTypeDefinition() == typeof(Nullable<>))
              {
                var underlyingType = Nullable.GetUnderlyingType(valueType);
                if (underlyingType != null)
                {
                  switch (underlyingType.Name)
                  {
                    case nameof(Int32):
                    case nameof(Int64):
                    case nameof(Decimal):
                    case nameof(Double):
                    case nameof(Single):
                      hasValue = Convert.ToDouble(value) > 0;
                      break;
                    case nameof(Boolean):
                      hasValue = Convert.ToBoolean(value);
                      break;
                    default:
                      hasValue = true;
                      break;
                  }
                }
              }
              else if (!value.Equals(Activator.CreateInstance(prop.PropertyType)))
              {
                hasValue = true;
              }
            }
            break;
        }

        if (hasValue) break;
      }

      if (!hasValue)
      {
        context.AddFailure(message);
      }
    });
  }
}