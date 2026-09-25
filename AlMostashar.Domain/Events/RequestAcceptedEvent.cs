using AlMostashar.Domain.Entities;
using AlMostashar.Domain.Entities.Common;

namespace AlMostashar.Domain.Events;

/// <summary>
/// يُرفع عند قبول طلب عميل ليتم إنشاء الفاتورة والإجراءات التابعة.
/// </summary>
public sealed class RequestAcceptedEvent : BaseEvent
{
    public RequestAcceptedEvent(
        ClientRequest request,
        decimal totalAmount,
        decimal platformFee,
        decimal lawyerAmount)
    {
        ClientRequestId = request.Id;
        ClientId = request.ClientId;
        LawyerId = request.LawyerServiceLawyerId;
        TotalAmount = totalAmount;
        PlatformFee = platformFee;
        LawyerAmount = lawyerAmount;
        RequestType = request is BroadcastRequest ? "Broadcast" : "Direct";
    }

    public int ClientRequestId { get; }
    public int ClientId { get; }
    public int? LawyerId { get; }
    public decimal TotalAmount { get; }
    public decimal PlatformFee { get; }
    public decimal LawyerAmount { get; }
    public string RequestType { get; }
}
