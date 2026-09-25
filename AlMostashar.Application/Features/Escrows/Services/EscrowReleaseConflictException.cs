namespace AlMostashar.Application.Features.Escrows.Services;

public sealed class EscrowReleaseConflictException : Exception
{
    public EscrowReleaseConflictException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
