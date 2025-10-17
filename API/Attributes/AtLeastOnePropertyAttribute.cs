using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace API.Attributes
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AtLeastOnePropertyAttribute : ValidationAttribute
    {
        public override bool IsValid(object? value)
        {
            if (value == null)
            {
                return true;
            }

            var type = value.GetType();
            var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);

            foreach (var property in properties)
            {
                if (property.GetValue(value, null) != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}
