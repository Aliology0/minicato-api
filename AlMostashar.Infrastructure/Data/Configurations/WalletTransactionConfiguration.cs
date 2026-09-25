using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class WalletTransactionConfiguration : IEntityTypeConfiguration<WalletTransaction>
{
    public void Configure(EntityTypeBuilder<WalletTransaction> builder)
    {
        // ─── Table ───
        builder.ToTable("WalletTransactions");

        // ─── Properties ───
        builder.Property(wt => wt.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(wt => wt.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(wt => wt.ReferenceType)
            .HasMaxLength(100);

        builder.Property(wt => wt.ReferenceId)
            .HasMaxLength(100);

        builder.Property(wt => wt.Description)
            .HasMaxLength(500);

        builder.Property(wt => wt.BalanceAfter)
            .HasColumnType("decimal(18,2)");

        // ─── Relationships ───

        // Optional many-to-one: WalletTransaction → Escrow
        builder.HasOne(wt => wt.Escrow)
            .WithMany(e => e.WalletTransactions)
            .HasForeignKey(wt => wt.EscrowId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wt => wt.WithdrawalRequest)
            .WithMany(wr => wr.WalletTransactions)
            .HasForeignKey(wt => wt.WithdrawalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        // ─── Indexes ───
        builder.HasIndex(wt => wt.WalletId);
        builder.HasIndex(wt => wt.EscrowId);
        builder.HasIndex(wt => wt.WithdrawalRequestId);
        builder.HasIndex(wt => wt.InvoiceId);
        builder.HasIndex(wt => wt.CaseId);
        builder.HasIndex(wt => wt.CreatedAt);
        builder.HasIndex(wt => wt.EscrowId)
            .IsUnique()
            .HasFilter("[EscrowId] IS NOT NULL AND [Type] = 'Credit'");
    }
}
