using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Entities.Common;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Events;

/// <summary>
/// Raised when a new LegalService is created on the platform.
/// </summary>
public sealed class LegalServiceCreatedEvent : BaseEvent
{
    public int LegalServiceId { get; }
    public string Title { get; }
    public ServiceType ServiceType { get; }
    public int AdminId { get; }

    public LegalServiceCreatedEvent(int legalServiceId, string title, ServiceType serviceType, int adminId)
    {
        LegalServiceId = legalServiceId;
        Title = title;
        ServiceType = serviceType;
        AdminId = adminId;
    }
}
