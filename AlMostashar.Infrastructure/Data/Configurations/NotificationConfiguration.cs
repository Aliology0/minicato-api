using AlMostashar.Domain.Entities;
using AlMostashar.Domain.ValueObject.Enum;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        // ─── Table ───
        builder.ToTable("Notifications");

        // ─── Properties ───
        builder.Property(n => n.Type)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(50);

        builder.Property(n => n.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(n => n.Description)
            .HasMaxLength(2000);

        // ─── Indexes ───
        builder.HasIndex(n => n.UserId);
    }
}
