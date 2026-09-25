using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        // ─── Table ───
        builder.ToTable("Wallets");

        // ─── Properties ───
        builder.Property(w => w.AvailableBalance)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(w => w.Total)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        // ─── Indexes ───
        builder.HasIndex(w => w.LawyerId)
            .IsUnique();

        // ─── Relationships ───

        // 1:N — Wallet → WalletTransaction
        builder.HasMany(w => w.WalletTransactions)
            .WithOne(wt => wt.Wallet)
            .HasForeignKey(wt => wt.WalletId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(w => w.WithdrawalRequests)
            .WithOne(wr => wr.Wallet)
            .HasForeignKey(wr => wr.WalletId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
