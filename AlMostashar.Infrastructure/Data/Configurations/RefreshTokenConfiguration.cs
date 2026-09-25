using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        // ─── Table ───
        builder.ToTable("RefreshTokens");

        // ─── Properties ───
        builder.Property(rt => rt.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(rt => rt.DeviceName)
            .HasMaxLength(200);

        // ─── Indexes ───
        builder.HasIndex(rt => rt.Token)
            .IsUnique();

        builder.HasIndex(rt => rt.UserId);

        // ─── Relationships ───

        // N:1 — RefreshToken → User
        builder.HasOne(rt => rt.User)
            .WithMany(u => u.RefreshTokens)
            .HasForeignKey(rt => rt.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
