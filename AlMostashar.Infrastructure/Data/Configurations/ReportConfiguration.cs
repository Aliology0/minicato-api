using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ReportConfiguration : IEntityTypeConfiguration<Report>
{
    public void Configure(EntityTypeBuilder<Report> builder)
    {
        // ─── Table ───
        builder.ToTable("Reports");

        // ─── Properties ───
        builder.Property(r => r.Reason)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(r => r.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(r => r.AdminNotes)
            .HasMaxLength(4000);

        // JSON-serialized list of decrypted chat messages submitted as evidence (nullable)
        // Messages are sent from the user's local device since server cannot decrypt E2E-encrypted chats
        builder.Property(r => r.AttachedMessages)
            .HasMaxLength(8000);

        // JSON-serialized list of S3 file keys for uploaded evidence attachments (nullable)
        builder.Property(r => r.AttachmentUrls)
            .HasMaxLength(4000);

        // ─── Indexes ───
        builder.HasIndex(r => r.ReporterId);
        builder.HasIndex(r => r.ReportedUserId);
        builder.HasIndex(r => r.CaseId);
        builder.HasIndex(r => r.ReviewedByAdminId);

        // ─── Relationships ───

        // N:1 — Report → User (Reporter)
        builder.HasOne(r => r.Reporter)
            .WithMany(u => u.ReportsFiled)
            .HasForeignKey(r => r.ReporterId)
            .OnDelete(DeleteBehavior.Restrict);

        // N:1 — Report → User (ReportedUser) — nullable (platform-level reports have no target user)
        builder.HasOne(r => r.ReportedUser)
            .WithMany(u => u.ReportsReceived)
            .HasForeignKey(r => r.ReportedUserId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // N:1 — Report → Case — nullable (report may not be tied to a case)
        builder.HasOne(r => r.Case)
            .WithMany(c => c.Reports)
            .HasForeignKey(r => r.CaseId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        // N:1 — Report → Admin (ReviewedBy, optional)
        builder.HasOne(r => r.ReviewedByAdmin)
            .WithMany(a => a.ReviewedReports)
            .HasForeignKey(r => r.ReviewedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
