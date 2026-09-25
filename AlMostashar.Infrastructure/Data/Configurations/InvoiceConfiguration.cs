using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        // ─── Table ───
        builder.ToTable("Invoices");

        // ─── Properties ───
        builder.Property(i => i.ReferenceNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(i => i.PlatformFee)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.LawyerAmount)
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.TotalAmount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(i => i.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        // ─── Indexes ───
        builder.HasIndex(i => i.ClientRequestId)
            .IsUnique();

        // ─── Relationships ───

        // 1:1 — Invoice → Payment
        builder.HasOne(i => i.Payment)
            .WithOne(p => p.Invoice)
            .HasForeignKey<Payment>(p => p.InvoiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
