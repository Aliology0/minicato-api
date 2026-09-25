using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class EscrowConfiguration : IEntityTypeConfiguration<Escrow>
{
    public void Configure(EntityTypeBuilder<Escrow> builder)
    {
        // ─── Table ───
        builder.ToTable("Escrows");

        // ─── Properties ───
        builder.Property(e => e.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(e => e.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // ─── Relationships ───

        // 1:1 ClientRequest → Escrow (existing)
        builder.HasOne(e => e.Request)
            .WithOne(r => r.Escrow)
            .HasForeignKey<Escrow>(e => e.RequestId)
            .OnDelete(DeleteBehavior.Restrict);

        // Optional 1:1 Payment → Escrow
        // FK is Escrow.PaymentId; nullable to support legacy/NotFunded escrows.
        builder.HasOne(e => e.Payment)
            .WithOne(p => p.Escrow)
            .HasForeignKey<Escrow>(e => e.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Indexes ───
        builder.HasIndex(e => e.RequestId)
            .IsUnique();

        // Filtered unique index on PaymentId for non-null values.
        // Ensures at most one escrow per payment.
        builder.HasIndex(e => e.PaymentId)
            .IsUnique()
            .HasFilter("[PaymentId] IS NOT NULL");
    }
}
