using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Api.DTOs
{
    /// <summary>
    /// Request body for generating a call token.
    /// </summary>
    public record GenerateCallTokenRequest(CallType CallType);
}
