using AlMostashar.Domain.ValueObject.Enum;

namespace AlMostashar.Domain.Entities;

public static class CaseFactory
{
    public static Case Create(ServiceType serviceType, int lawyerId, string title, string description = "")
    {
        var normalizedServiceType = NormalizeServiceType(serviceType);
        var now = DateTime.UtcNow;
        var referenceNumber = $"CAS-{now:yyyyMMddHHmmss}-{Random.Shared.Next(1000, 9999)}";

        return normalizedServiceType switch
        {
            ServiceType.IntellectualProperty => new IntellectualPropertyCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.LegalTranslation => new LegalTranslationCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.DueDiligence => new DueDiligenceCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.DebtCollection => new DebtCollectionCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.Mediation => new MediationCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.LegalReview => new LegalReviewCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.Compliance => new ComplianceCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.Consultation => new ConsultationCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.Contract => new ContractCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.CompanyFormation => new CompanyFormationCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            ServiceType.Lawsuit => new LawsuitCase
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            },
            // All other service types fall back to a plain base case.
            _ => new Case
            {
                Title = title,
                Description = description,
                ServiceType = normalizedServiceType,
                ReferenceNumber = referenceNumber,
                LawyerId = lawyerId,
                CreatedAt = now,
                Status = CaseStatus.Open
            }
        };
    }

    public static ServiceType NormalizeServiceType(ServiceType serviceType)
        => Enum.IsDefined(serviceType) ? serviceType : ServiceType.Base;
}
