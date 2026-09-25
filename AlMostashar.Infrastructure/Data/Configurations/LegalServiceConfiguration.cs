using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class LegalServiceConfiguration : IEntityTypeConfiguration<LegalService>
{
    public void Configure(EntityTypeBuilder<LegalService> builder)
    {
        builder.ToTable("LegalServices");

        // ─── UI & Display ───
        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(s => s.Summary)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.FullDescription)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.IconUrl)
            .HasMaxLength(500);

        // ─── Classification ───
        builder.Property(s => s.ServiceType)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(s => s.ServiceType)
            .IsUnique();

        // ─── Operational Details ───
        builder.Property(s => s.RequiredDocuments)
            .HasMaxLength(4000);

        builder.Property(s => s.ExpectedDuration)
            .HasMaxLength(50);

        // ─── Admin Control & Audit ───
        builder.Property(s => s.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

        builder.Property(s => s.AdminId)
            .IsRequired();

        builder.HasOne(s => s.Admin)
            .WithMany()
            .HasForeignKey(s => s.AdminId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(s => s.CreatedAt)
            .IsRequired();
    }
}
