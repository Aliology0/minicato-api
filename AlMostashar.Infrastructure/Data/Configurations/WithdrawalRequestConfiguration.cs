using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class WithdrawalRequestConfiguration : IEntityTypeConfiguration<WithdrawalRequest>
{
    public void Configure(EntityTypeBuilder<WithdrawalRequest> builder)
    {
        builder.ToTable("WithdrawalRequests");

        builder.Property(wr => wr.Amount)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(wr => wr.Method)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(wr => wr.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(wr => wr.AccountDetailsEncrypted)
            .IsRequired()
            .HasMaxLength(2048);

        builder.Property(wr => wr.AccountDetailsMasked)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(wr => wr.AdminNotes)
            .HasMaxLength(1000);

        builder.Property(wr => wr.RejectionReason)
            .HasMaxLength(1000);

        builder.Property(wr => wr.PayoutReference)
            .HasMaxLength(300);

        builder.Property(wr => wr.PayoutProvider)
            .HasMaxLength(100);

        builder.HasOne(wr => wr.Lawyer)
            .WithMany()
            .HasForeignKey(wr => wr.LawyerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(wr => wr.Wallet)
            .WithMany(w => w.WithdrawalRequests)
            .HasForeignKey(wr => wr.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(wr => wr.WalletTransactions)
            .WithOne(wt => wt.WithdrawalRequest)
            .HasForeignKey(wt => wt.WithdrawalRequestId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(wr => wr.LawyerId);
        builder.HasIndex(wr => wr.WalletId);
        builder.HasIndex(wr => wr.Status);
        builder.HasIndex(wr => wr.RequestedAt);
        builder.HasIndex(wr => wr.PaidAt);
        builder.HasIndex(wr => new { wr.WalletId, wr.Status });
        builder.HasIndex(wr => wr.PayoutReference)
            .IsUnique()
            .HasFilter("[PayoutReference] IS NOT NULL");
        builder.HasIndex(wr => wr.WalletTransactionId);
    }
}
