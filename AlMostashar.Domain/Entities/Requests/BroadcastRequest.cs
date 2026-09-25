namespace AlMostashar.Domain.Entities;

// EERD: Specialization of ClientRequest
public class BroadcastRequest : ClientRequest
{
    public decimal Budget { get; set; }
}
