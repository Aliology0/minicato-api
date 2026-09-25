namespace AlMostashar.Domain.Shared;

/// <summary>
/// Represents the result of an operation, containing a success flag, an optional error, and an optional value.
/// </summary>
/// <typeparam name="TValue">The type of the returned value.</typeparam>
public class Result<TValue>
{
    public bool IsSuccess { get; }

    public Error? Error { get; }

    public TValue? Value { get; }

    protected Result(TValue? value, bool isSuccess, Error? error=null)
    {
        if (isSuccess &&  error is not null)
        {
            throw new InvalidOperationException("A success result cannot have an error.");
        }

        if (!isSuccess &&  error is null)
        {
            throw new InvalidOperationException("A failure result must have an error.");
        }

        Value = value;
        IsSuccess = isSuccess;
        Error = error;
    }

    /// <summary>
    /// Creates a success result carrying the target value.
    /// </summary>
    public static Result<TValue> Success(TValue value) => new(value, true);

    /// <summary>
    /// Creates a generic failure result without a value, containing the relevant error.
    /// </summary>
    public static Result<TValue> Failure(Error error) => new(default, false, error);
    
}
