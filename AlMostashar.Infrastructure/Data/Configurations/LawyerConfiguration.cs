using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class LawyerConfiguration : IEntityTypeConfiguration<Lawyer>
{
    public void Configure(EntityTypeBuilder<Lawyer> builder)
    {
        // ─── Properties ───
        builder.Property(l => l.PhoneNo)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(l => l.Governorate)
            .HasMaxLength(100);

        builder.Property(l => l.City)
            .HasMaxLength(100);


        builder.Property(l => l.Bio)
            .HasMaxLength(1000);

        builder.Property(l => l.About)
            .HasMaxLength(2000);

        builder.Property(l => l.SSN_Url)
            .HasMaxLength(500);

        builder.Property(l => l.SyndicateCardUrl)
            .HasMaxLength(500);

        builder.Property(l => l.PracticeCertificatesUrl)
            .HasMaxLength(500);

        // ─── Indexes ───
        builder.HasIndex(l => l.VerifiedByAdminId);
        builder.HasIndex(l => l.SyndicateId)
            .IsUnique();

        // ─── Relationships ───

        // 1:1 — Lawyer → Wallet (Owns)
        builder.HasOne(l => l.Wallet)
            .WithOne(w => w.Lawyer)
            .HasForeignKey<Wallet>(w => w.LawyerId)
            .OnDelete(DeleteBehavior.Cascade);

        // M:N — Lawyer ↔ Service via LawyerService
        builder.HasMany(l => l.LawyerServices)
            .WithOne(ls => ls.Lawyer)
            .HasForeignKey(ls => ls.LawyerId)
            .OnDelete(DeleteBehavior.Cascade);

        // M:N — Lawyer ↔ LawyerSpecialization
        builder.HasMany(l => l.LawyerSpecializations)
            .WithMany(s => s.Lawyers)
            .UsingEntity(j => j.ToTable("LawyerSpecializationsMapping"));
    }
}
