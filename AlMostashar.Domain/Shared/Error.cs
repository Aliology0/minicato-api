namespace AlMostashar.Domain.Shared;

/// <summary>
/// Represents a simple error with a code and a message.
/// </summary>
/// <param name="Code">A unique string code indicating the type of error (e.g., "User.NotFound").</param>
/// <param name="Message">A human-readable error message explaining what happened.</param>
public record Error(string Code, string Message, object? Details= null)
{
    /// <summary>
    /// Represents an empty error, indicating success or no error.
    /// </summary>
    public static readonly Error None = new(string.Empty, string.Empty);
}

/// <summary>
/// Represents an error with strongly typed extra details.
/// </summary>
/// <typeparam name="TDetails">The type of the details.</typeparam>
