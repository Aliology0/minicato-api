using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Api.DTOs
{
    /// <summary>
    /// Request body for updating a call status.
    /// </summary>
    public record UpdateCallStatusRequest(CallStatus Status);
}
