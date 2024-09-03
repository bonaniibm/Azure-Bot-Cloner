using System.ComponentModel.DataAnnotations;

public class RequiredIfAttribute : ValidationAttribute
{
    private readonly string _propertyName;
    private readonly object _desiredValue;

    public RequiredIfAttribute(string propertyName, object desiredValue)
    {
        _propertyName = propertyName;
        _desiredValue = desiredValue;
    }

    protected override ValidationResult IsValid(object value, ValidationContext validationContext)
    {
        var instance = validationContext.ObjectInstance;
        var type = instance.GetType();
        var propertyValue = type.GetProperty(_propertyName).GetValue(instance, null);

        if (propertyValue.ToString() == _desiredValue.ToString())
        {
            if (value == null || (value as string)?.Trim().Length == 0)
            {
                return new ValidationResult(ErrorMessage);
            }
        }

        return ValidationResult.Success;
    }
}