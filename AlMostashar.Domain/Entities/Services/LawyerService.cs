using AlMostashar.Domain.Shared;

namespace AlMostashar.Domain.Entities;

/// <summary>
/// M:N junction between Lawyer and LegalService, with relationship attributes.
/// </summary>
public class LawyerService
{
    // ── Properties (private setters) ──────────────────────────────────
    public int LawyerId { get; private set; }
    public Lawyer Lawyer { get; private set; } = null!;

    public int LegalServiceId { get; private set; }
    public LegalService LegalService { get; private set; } = null!;

    public decimal Price { get; private set; }
    public string Duration { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────
    public ICollection<ClientRequest> Requests { get; private set; } = new List<ClientRequest>();
    public ICollection<Feedback> Feedbacks { get; private set; } = new List<Feedback>();

    // ── EF Core constructor ───────────────────────────────────────────
    private LawyerService() { }

    // ── Factory ───────────────────────────────────────────────────────
    public static (LawyerService? Entity, Error? Error) Create(
        int lawyerId,
        int legalServiceId,
        decimal price,
        string duration)
    {
        if (lawyerId <= 0)
            return (null, DomainErrors.InvalidId(nameof(LawyerId)));

        if (legalServiceId <= 0)
            return (null, DomainErrors.InvalidId(nameof(LegalServiceId)));

        if (price < 0)
            return (null, DomainErrors.Negative(nameof(Price)));

        if (string.IsNullOrWhiteSpace(duration))
            return (null, DomainErrors.NullOrEmpty(nameof(Duration)));

        var service = new LawyerService
        {
            LawyerId = lawyerId,
            LegalServiceId = legalServiceId,
            Price = price,
            Duration = duration,
            IsActive = true
        };

        return (service, null);
    }

    // ── Behavior ──────────────────────────────────────────────────────

    public Error? UpdatePrice(decimal newPrice)
    {
        if (newPrice < 0)
            return DomainErrors.Negative(nameof(Price));

        Price = newPrice;
        return null;
    }

    public Error? UpdateDuration(string newDuration)
    {
        if (string.IsNullOrWhiteSpace(newDuration))
            return DomainErrors.NullOrEmpty(nameof(Duration));

        Duration = newDuration;
        return null;
    }

    public Error? Activate()
    {
        if (IsActive)
            return DomainErrors.AlreadyInState(nameof(IsActive), "active");

        IsActive = true;
        return null;
    }

    public Error? Deactivate()
    {
        if (!IsActive)
            return DomainErrors.AlreadyInState(nameof(IsActive), "inactive");

        IsActive = false;
        return null;
    }
}
