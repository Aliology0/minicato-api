using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        // ─── Table ───
        builder.ToTable("Payments");

        // ─── Properties ───
        builder.Property(p => p.TransactionId)
            .IsRequired(false);

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.PaymentMethod)
            .IsRequired(false)
            .HasMaxLength(50);

        builder.Property(p => p.PaymentProvider)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(p => p.IntentionId)
            .IsRequired(false)
            .HasMaxLength(200);

        builder.Property(p => p.GatewayResponse)
            .IsRequired(false);

        builder.Property(p => p.ProviderFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.NetAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.RefundedAmount)
            .HasColumnType("decimal(18,2)");

        // ─── Indexes ───

        // Enforce 1:1 Invoice → Payment at the database level
        builder.HasIndex(p => p.InvoiceId)
            .IsUnique();

        // Prevent duplicate transaction IDs (filtered: only non-null)
        builder.HasIndex(p => p.TransactionId)
            .IsUnique()
            .HasFilter("[TransactionId] IS NOT NULL");
    }
}
