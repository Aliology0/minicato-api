using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class CaseDocumentsConfiguration : IEntityTypeConfiguration<CaseDocuments>
{
    public void Configure(EntityTypeBuilder<CaseDocuments> builder)
    {
        // ─── Table ───
        builder.ToTable("CaseDocuments");

        // ─── Properties ───
        builder.Property(d => d.DocumentName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(d => d.DocumentUrl)
            .IsRequired()
            .HasMaxLength(500);

        // ─── Indexes ───
        builder.HasIndex(d => d.CaseId);
        builder.HasIndex(d => d.ClientRequestId);
        builder.HasIndex(d => d.ReportId);
        builder.HasIndex(d => d.UploadedByUserId);
        builder.Property(d => d.CleanupClaimToken)
            .HasMaxLength(36)
            .IsConcurrencyToken();
        builder.HasIndex(d => new { d.ClientRequestId, d.CaseId, d.ReportId, d.CreatedAt, d.CleanupClaimToken });

        // ─── Relationships ───

        // N:1 — CaseDocuments → Case (nullable)
        builder.HasOne(d => d.Case)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.CaseId)
            .OnDelete(DeleteBehavior.Restrict);

        // N:1 — CaseDocuments → ClientRequest (nullable)
        builder.HasOne(d => d.ClientRequest)
            .WithMany(cr => cr.Documents)
            .HasForeignKey(d => d.ClientRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(d => d.Report)
            .WithMany(r => r.Attachments)
            .HasForeignKey(d => d.ReportId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

