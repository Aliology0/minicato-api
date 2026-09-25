using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public class Invoice : BaseEntity
{
    public string ReferenceNumber { get; set; }
    public decimal PlatformFee { get; set; }
    public decimal LawyerAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public InvoiceStatus Status { get; set; }
    public DateTime DueDate { get; set; }
    public DateTime IssueDate { get; set; }
    public DateTime PaidAt { get; set; }

    // FK — Step 3: 1:1 (ClientRequest → Invoice)
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; }

    // Navigation — Step 3: 1:1 (Invoice → Payment)
    public Payment Payment { get; set; }
}
