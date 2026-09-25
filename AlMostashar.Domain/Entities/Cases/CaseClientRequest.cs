namespace AlMostashar.Domain.Entities;

public class CaseClientRequest
{
    // The ID from the case table
    public int CaseId { get; set; }

    // The ID from the client request table
    public int ClientRequestId { get; set; }

    // Date and time when we added this link
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Connect this class to the Case class
    public virtual Case Case { get; set; }

    // Connect this class to the ClientRequest class
    public virtual ClientRequest ClientRequest { get; set; }
}
