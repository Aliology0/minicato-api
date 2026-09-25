namespace AlMostashar.Application.Common.Exceptions;

public sealed class LegalAiServiceUnavailableException : Exception
{
    public LegalAiServiceUnavailableException(string message, Exception? innerException = null)
        : base(message, innerException)
    {
    }
}
