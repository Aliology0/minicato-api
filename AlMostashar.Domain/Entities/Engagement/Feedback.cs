namespace AlMostashar.Domain.Entities;

// Step 2: Weak Entity of ClientRequest (1:1)
public class Feedback : BaseEntity
{
    public double Rate { get; set; }
    public string Content { get; set; }

    // FK — Step 3: 1:1 (ClientRequest → Feedback)
    public int ClientRequestId { get; set; }
    public ClientRequest ClientRequest { get; set; }
    public int? LawyerServiceLawyerId { get; set; }
    public int? LawyerServiceLegalServiceId { get; set; }
    public LawyerService? LawyersServices { get; set; }

}
