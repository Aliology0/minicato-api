using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class WebhookLogConfiguration : IEntityTypeConfiguration<WebhookLog>
{
    public void Configure(EntityTypeBuilder<WebhookLog> builder)
    {
        // ─── Table ───
        builder.ToTable("WebhookLogs");

        // ─── Properties ───

        // Full raw JSON — no length restriction (nvarchar(max))
        builder.Property(w => w.RawPayload)
            .IsRequired();

        builder.Property(w => w.ProcessingResult)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(w => w.ErrorMessage)
            .IsRequired(false)
            .HasMaxLength(500);

        builder.Property(w => w.ReceivedAt)
            .IsRequired();

        // ─── Indexes ───
        builder.HasIndex(w => w.TransactionId);
        builder.HasIndex(w => w.PaymentId);

        // ─── Relationships ───
        builder.HasOne(w => w.Payment)
            .WithMany()
            .HasForeignKey(w => w.PaymentId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
