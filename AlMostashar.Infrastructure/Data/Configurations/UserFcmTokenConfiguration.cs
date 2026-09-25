using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class UserFcmTokenConfiguration : IEntityTypeConfiguration<UserFcmToken>
{
    public void Configure(EntityTypeBuilder<UserFcmToken> builder)
    {
        // ─── Table ───
        builder.ToTable("UserFcmTokens");

        // ─── Properties ───
        builder.Property(t => t.Token)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(t => t.DeviceType)
            .HasMaxLength(20);

        // ─── Indexes ───
        builder.HasIndex(t => t.UserId);

        // Token should be unique per user (one row per device registration)
        builder.HasIndex(t => t.Token).IsUnique();

        // ─── Relationships ───

        // N:1 — UserFcmToken → User
        builder.HasOne(t => t.User)
            .WithMany(u => u.FcmTokens)
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
