using AlMostashar.Domain.Shared;
using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

/// <summary>
/// Represents a legal service type available on the platform (catalog entry).
/// This is a flat entity — no inheritance. Operational details belong on Case.
/// </summary>
public class LegalService : BaseEntity
{
    // ── UI & Display ──────────────────────────────────────────────────

    /// <summary>The main name of the service.</summary>
    public string Title { get; private set; } = string.Empty;

    /// <summary>A short text for UI cards.</summary>
    public string Summary { get; private set; } = string.Empty;

    /// <summary>Full details for the service page (can be HTML or Markdown).</summary>
    public string FullDescription { get; private set; } = string.Empty;

    /// <summary>Link to the image or icon for the service.</summary>
    public string? IconUrl { get; private set; }

    // ── Classification ────────────────────────────────────────────────

    public ServiceType ServiceType { get; private set; }

    // ── Operational Details ────────────────────────────────────────────

    /// <summary>What the client needs to prepare (e.g., ID, old contracts). Can be stored as JSON.</summary>
    public string? RequiredDocuments { get; private set; }

    /// <summary>A general text showing how long this service usually takes.</summary>
    public string? ExpectedDuration { get; private set; }

    // ── Admin Control & Audit ─────────────────────────────────────────

    /// <summary>If false, the service will be hidden from the platform.</summary>
    public bool IsActive { get; private set; } = true;

    /// <summary>The admin who created this service.</summary>
    public int AdminId { get; private set; }
    public Admin Admin { get; private set; } = null!;

    public DateTime CreatedAt { get; private set; }

    // ── Navigation ────────────────────────────────────────────────────

    /// <summary>M:N (Lawyer ↔ Service via LawyerService)</summary>
    public ICollection<LawyerService> LawyerServices { get; private set; } = new List<LawyerService>();
    public ICollection<RequestOffer> RequestOffers { get; private set; } = new List<RequestOffer>();

    // ── EF Core constructor ───────────────────────────────────────────
    private LegalService() { }

    // ── Factory ───────────────────────────────────────────────────────
    public static (LegalService? Entity, Error? Error) Create(
        string title,
        string summary,
        string fullDescription,
        ServiceType serviceType,
        int adminId,
        string? iconUrl = null,
        string? requiredDocuments = null,
        string? expectedDuration = null)
    {
        if (string.IsNullOrWhiteSpace(title))
            return (null, DomainErrors.NullOrEmpty(nameof(Title)));

        if (string.IsNullOrWhiteSpace(summary))
            return (null, DomainErrors.NullOrEmpty(nameof(Summary)));

        if (string.IsNullOrWhiteSpace(fullDescription))
            return (null, DomainErrors.NullOrEmpty(nameof(FullDescription)));

        if (!System.Enum.IsDefined(serviceType))
            return (null, DomainErrors.InvalidEnumValue(nameof(ServiceType)));

        if (adminId <= 0)
            return (null, DomainErrors.InvalidId(nameof(AdminId)));

        var service = new LegalService
        {
            Title = title,
            Summary = summary,
            FullDescription = fullDescription,
            ServiceType = serviceType,
            AdminId = adminId,
            IconUrl = iconUrl,
            RequiredDocuments = requiredDocuments,
            ExpectedDuration = expectedDuration,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
        };
        return (service, null);
    }

    // ── Update methods ────────────────────────────────────────────────

    public Error? UpdateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
            return DomainErrors.NullOrEmpty(nameof(Title));

        Title = title;
        return null;
    }

    public Error? UpdateSummary(string summary)
    {
        if (string.IsNullOrWhiteSpace(summary))
            return DomainErrors.NullOrEmpty(nameof(Summary));

        Summary = summary;
        return null;
    }

    public Error? UpdateFullDescription(string fullDescription)
    {
        if (string.IsNullOrWhiteSpace(fullDescription))
            return DomainErrors.NullOrEmpty(nameof(FullDescription));

        FullDescription = fullDescription;
        return null;
    }

    public void UpdateIconUrl(string? iconUrl) => IconUrl = iconUrl;

    public void UpdateRequiredDocuments(string? requiredDocuments) => RequiredDocuments = requiredDocuments;

    public void UpdateExpectedDuration(string? expectedDuration) => ExpectedDuration = expectedDuration;

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
