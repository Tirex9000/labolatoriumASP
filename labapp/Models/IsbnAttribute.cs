using System.ComponentModel.DataAnnotations;

namespace labapp.Models;

public class IsbnAttribute : ValidationAttribute
{
    private bool _requiredControlSum;

    public IsbnAttribute(bool requiredControlSum)
    {
        _requiredControlSum = requiredControlSum;
    }

    public override bool IsValid(object? value)
    {
        if (value == null)
        {
            ErrorMessage = "Value cannot be null";
            return false;
        }
        if (value is not string isbn)
        {
            ErrorMessage = $"Value must be a string";
            return false;
        }

        if (isbn.Length is not (10 or 13))
        {
            ErrorMessage = "ISBN must be 10 or 13";
            return  false;
        }
        
        return true;
    }
}