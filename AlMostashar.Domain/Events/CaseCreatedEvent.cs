using AlMostashar.Domain.Entities.Common;

namespace AlMostashar.Domain.Events;

/// <summary>
/// Raised after a Case is created. Used to trigger chat creation and other side-effects.
/// </summary>
public sealed class CaseCreatedEvent : BaseEvent
{
    public int CaseId { get; }
    public int LawyerId { get; }
    public int ClientId { get; }

    public CaseCreatedEvent(int caseId, int lawyerId, int clientId)
    {
        CaseId = caseId;
        LawyerId = lawyerId;
        ClientId = clientId;
    }
}
