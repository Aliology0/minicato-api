using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CaseClientRequestConfiguration : IEntityTypeConfiguration<CaseClientRequest>
{
    public void Configure(EntityTypeBuilder<CaseClientRequest> builder)
    {
        // ─── Table ───
        builder.ToTable("CaseClientRequests");

        // ─── Composite Key ───
        builder.HasKey(ccr => new { ccr.CaseId, ccr.ClientRequestId });

        // ─── Indexes (enforce 1:1 on ClientRequest side) ───
        builder.HasIndex(ccr => ccr.ClientRequestId)
            .IsUnique();

        // ─── Relationships ───

        // 1:1 — CaseClientRequest → Case
        builder.HasOne(ccr => ccr.Case)
            .WithOne(c => c.CaseClientRequest)
            .HasForeignKey<CaseClientRequest>(ccr => ccr.CaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 — CaseClientRequest → ClientRequest
        builder.HasOne(ccr => ccr.ClientRequest)
            .WithOne(cr => cr.CaseClientRequest)
            .HasForeignKey<CaseClientRequest>(ccr => ccr.ClientRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
