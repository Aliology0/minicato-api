using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class PaymentWalletTransactionConfiguration : IEntityTypeConfiguration<PaymentWalletTransaction>
{
    public void Configure(EntityTypeBuilder<PaymentWalletTransaction> builder)
    {
        // ─── Table ───
        builder.ToTable("PaymentWalletTransactions");

        // ─── Composite Key ───
        builder.HasKey(pwt => new { pwt.PaymentId, pwt.WalletTransactionId });

        // ─── Indexes (enforce 1:1 on WalletTransaction side) ───
        builder.HasIndex(pwt => pwt.WalletTransactionId)
            .IsUnique();

        // ─── Relationships ───

        // 1:1 — PaymentWalletTransaction → Payment
        builder.HasOne(pwt => pwt.Payment)
            .WithOne(p => p.PaymentWalletTransaction)
            .HasForeignKey<PaymentWalletTransaction>(pwt => pwt.PaymentId)
            .OnDelete(DeleteBehavior.Restrict);

        // 1:1 — PaymentWalletTransaction → WalletTransaction
        builder.HasOne(pwt => pwt.WalletTransaction)
            .WithOne(wt => wt.PaymentWalletTransaction)
            .HasForeignKey<PaymentWalletTransaction>(pwt => pwt.WalletTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
