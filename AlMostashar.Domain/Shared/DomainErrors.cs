namespace AlMostashar.Domain.Shared;

/// <summary>
/// Generic, reusable domain errors.
/// Use the field/entity parameters to contextualize each error at the call site.
/// </summary>
public static class DomainErrors
{
    public static Error NullOrEmpty(string fieldName)
        => new($"Domain.NullOrEmpty", $"'{fieldName}' must not be null or empty.");

    public static Error InvalidId(string fieldName)
        => new($"Domain.InvalidId", $"'{fieldName}' must be a positive integer.");

    public static Error NegativeOrZero(string fieldName)
        => new($"Domain.NegativeOrZero", $"'{fieldName}' must be greater than zero.");

    public static Error Negative(string fieldName)
        => new($"Domain.Negative", $"'{fieldName}' must not be negative.");

    public static Error PastDate(string fieldName)
        => new($"Domain.PastDate", $"'{fieldName}' must not be in the past.");

    public static Error InvalidEnumValue(string fieldName)
        => new($"Domain.InvalidEnumValue", $"'{fieldName}' has an invalid value.");

    public static Error InvalidTransition(string fieldName, string from, string to)
        => new($"Domain.InvalidTransition", $"Cannot transition '{fieldName}' from '{from}' to '{to}'.");

    public static Error OutOfRange(string fieldName, string min, string max)
        => new($"Domain.OutOfRange", $"'{fieldName}' must be between {min} and {max}.");

    public static Error TooLong(string fieldName, int maxLength)
        => new($"Domain.TooLong", $"'{fieldName}' must not exceed {maxLength} characters.");

    public static Error AlreadyInState(string fieldName, string state)
        => new($"Domain.AlreadyInState", $"'{fieldName}' is already '{state}'.");
}
