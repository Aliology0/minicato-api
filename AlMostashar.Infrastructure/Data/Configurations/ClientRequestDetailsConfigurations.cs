using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public sealed class ConsultationRequestDetailsConfiguration : IEntityTypeConfiguration<ConsultationRequestDetails>
{
    public void Configure(EntityTypeBuilder<ConsultationRequestDetails> b)
    {
        b.HasKey(x => x.ClientRequestId);
        b.HasOne(x => x.ClientRequest).WithOne(x => x.ConsultationDetails).HasForeignKey<ConsultationRequestDetails>(x => x.ClientRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.LegalBranch).HasMaxLength(100).IsRequired();
        b.Property(x => x.CommunicationMethod).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.ConsultationSummary).HasMaxLength(2000);
        b.HasIndex(x => x.LegalBranch);
    }
}

public sealed class ContractRequestDetailsConfiguration : IEntityTypeConfiguration<ContractRequestDetails>
{
    public void Configure(EntityTypeBuilder<ContractRequestDetails> b)
    {
        b.HasKey(x => x.ClientRequestId);
        b.HasOne(x => x.ClientRequest).WithOne(x => x.ContractDetails).HasForeignKey<ContractRequestDetails>(x => x.ClientRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.ContractType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.ContractRequestType).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.Language).HasMaxLength(50).IsRequired();
        b.Property(x => x.OtherPartyName).HasMaxLength(200);
        b.HasIndex(x => x.ContractType);
    }
}

public sealed class LawsuitRequestDetailsConfiguration : IEntityTypeConfiguration<LawsuitRequestDetails>
{
    public void Configure(EntityTypeBuilder<LawsuitRequestDetails> b)
    {
        b.HasKey(x => x.ClientRequestId);
        b.HasOne(x => x.ClientRequest).WithOne(x => x.LawsuitDetails).HasForeignKey<LawsuitRequestDetails>(x => x.ClientRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.LegalBranch).HasMaxLength(100).IsRequired();
        b.Property(x => x.CourtName).HasMaxLength(200); b.Property(x => x.CaseNumber).HasMaxLength(100);
        b.Property(x => x.LawsuitStatus).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.ClientRole).HasConversion<string>().HasMaxLength(30);
        b.Property(x => x.OpponentName).HasMaxLength(200);
        b.HasIndex(x => x.LegalBranch); b.HasIndex(x => x.CourtName); b.HasIndex(x => x.CaseNumber);
    }
}

public sealed class CompanyFormationRequestDetailsConfiguration : IEntityTypeConfiguration<CompanyFormationRequestDetails>
{
    public void Configure(EntityTypeBuilder<CompanyFormationRequestDetails> b)
    {
        b.HasKey(x => x.ClientRequestId);
        b.HasOne(x => x.ClientRequest).WithOne(x => x.CompanyFormationDetails).HasForeignKey<CompanyFormationRequestDetails>(x => x.ClientRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.CompanyType).HasConversion<string>().HasMaxLength(50);
        b.Property(x => x.BusinessActivity).HasMaxLength(500).IsRequired(); b.Property(x => x.CapitalAmount).HasPrecision(18, 2);
        b.Property(x => x.ProposedCompanyName).HasMaxLength(200); b.HasIndex(x => x.CompanyType);
    }
}

public sealed class GenericRequestDetailsConfiguration : IEntityTypeConfiguration<GenericRequestDetails>
{
    public void Configure(EntityTypeBuilder<GenericRequestDetails> b)
    {
        b.HasKey(x => x.ClientRequestId);
        b.HasOne(x => x.ClientRequest).WithOne(x => x.GenericDetails).HasForeignKey<GenericRequestDetails>(x => x.ClientRequestId).OnDelete(DeleteBehavior.Cascade);
        b.Property(x => x.LegalBranch).HasMaxLength(100); b.Property(x => x.Summary).HasMaxLength(2000);
        b.Property(x => x.DesiredOutcome).HasMaxLength(1000); b.Property(x => x.ImportantDates).HasMaxLength(1000);
        b.HasIndex(x => x.LegalBranch);
    }
}
