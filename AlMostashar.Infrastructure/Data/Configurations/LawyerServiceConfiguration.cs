using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class LawyerServiceConfiguration : IEntityTypeConfiguration<LawyerService>
{
    public void Configure(EntityTypeBuilder<LawyerService> builder)
    {
        // ─── Table ───
        builder.ToTable("LawyerServices");

        // ─── Composite Primary Key ───
        builder.HasKey(ls => new { ls.LawyerId, ls.LegalServiceId });

        // ─── Properties ───
        builder.Property(ls => ls.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(ls => ls.Duration)
            .HasMaxLength(100);

        // ─── Indexes ───
        builder.HasIndex(ls => ls.LawyerId);
        builder.HasIndex(ls => ls.LegalServiceId);

        // ─── Relationships ───

        // M:N — LawyerService → Service
        builder.HasOne(ls => ls.LegalService)
            .WithMany(s => s.LawyerServices)
            .HasForeignKey(ls => ls.LegalServiceId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
