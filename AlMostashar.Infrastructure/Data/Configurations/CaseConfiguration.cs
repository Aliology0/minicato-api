using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CaseConfiguration : IEntityTypeConfiguration<Case>
{
    public void Configure(EntityTypeBuilder<Case> builder)
    {
        // ─── Table & TPH Discriminator ───
        builder.ToTable("Cases");

        builder.HasDiscriminator(c => c.ServiceType)
            .HasValue<Case>(ServiceType.Base)
            .HasValue<IntellectualPropertyCase>(ServiceType.IntellectualProperty)
            .HasValue<LegalTranslationCase>(ServiceType.LegalTranslation)
            .HasValue<DueDiligenceCase>(ServiceType.DueDiligence)
            .HasValue<DebtCollectionCase>(ServiceType.DebtCollection)
            .HasValue<MediationCase>(ServiceType.Mediation)
            .HasValue<LegalReviewCase>(ServiceType.LegalReview)
            .HasValue<ComplianceCase>(ServiceType.Compliance)
            .HasValue<ConsultationCase>(ServiceType.Consultation)
            .HasValue<ContractCase>(ServiceType.Contract)
            .HasValue<CompanyFormationCase>(ServiceType.CompanyFormation)
            .HasValue<LawsuitCase>(ServiceType.Lawsuit);

        // ─── Base Properties ───
        builder.Property(c => c.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(c => c.ClientName)
            .HasMaxLength(200);

        builder.Property(c => c.CancellationReason)
            .HasMaxLength(1000);

        builder.Property(c => c.Description)
            .HasMaxLength(4000);

        builder.Property(c => c.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // Note: ServiceType is the TPH discriminator — EF Core manages its column type automatically.
        // Do NOT apply HasConversion<string>() on the discriminator.
        builder.Property(c => c.ServiceType)
            .IsRequired();

        // ─── Relationships ───

        // 1:N — Case → CaseTimeline
        builder.HasMany(c => c.Timelines)
            .WithOne(t => t.Case)
            .HasForeignKey(t => t.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N — Case → CaseNotes
        builder.HasMany(c => c.Notes)
            .WithOne(n => n.Case)
            .HasForeignKey(n => n.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N — Case → CaseDocuments
        builder.HasMany(c => c.Documents)
            .WithOne(d => d.Case)
            .HasForeignKey(d => d.CaseId)
            .OnDelete(DeleteBehavior.Cascade);

        // ─── Indexes ───
        builder.HasIndex(c => c.LawyerId);

        // N:1 — Case → Lawyer (mandatory from Case, partial from Lawyer)
        builder.HasOne(c => c.Lawyer)
            .WithMany(l => l.Cases)
            .HasForeignKey(c => c.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

// ─── Subtype Configurations ───

public class ConsultationCaseConfiguration : IEntityTypeConfiguration<ConsultationCase>
{
    public void Configure(EntityTypeBuilder<ConsultationCase> builder)
    {
        builder.Property(c => c.CommunicationMethod)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.LegalBranch)
            .HasColumnName("LegalBranch")
            .HasMaxLength(200);

        builder.Property(c => c.ConsultationSummary)
            .HasMaxLength(4000);
    }
}

public class ContractCaseConfiguration : IEntityTypeConfiguration<ContractCase>
{
    public void Configure(EntityTypeBuilder<ContractCase> builder)
    {
        builder.Property(c => c.ContractType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.Language)
            .HasMaxLength(50);
    }
}

public class CompanyFormationCaseConfiguration : IEntityTypeConfiguration<CompanyFormationCase>
{
    public void Configure(EntityTypeBuilder<CompanyFormationCase> builder)
    {
        builder.Property(c => c.CompanyType)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.CommercialRegistrationNo)
            .HasMaxLength(100);

        builder.Property(c => c.CapitalAmount)
            .HasColumnType("decimal(18,2)");
    }
}

public class LawsuitCaseConfiguration : IEntityTypeConfiguration<LawsuitCase>
{
    public void Configure(EntityTypeBuilder<LawsuitCase> builder)
    {
        builder.Property(c => c.LegalBranch)
            .HasColumnName("LegalBranch")
            .HasMaxLength(200);

        builder.Property(c => c.CourtName)
            .HasMaxLength(200);

        builder.Property(c => c.CaseNumber)
            .HasMaxLength(50);

        builder.Property(c => c.LawsuitStatus)
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(c => c.ClientRole)
            .HasConversion<string>()
            .HasMaxLength(50);
    }
}
