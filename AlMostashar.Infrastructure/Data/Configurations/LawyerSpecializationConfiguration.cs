using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class LawyerSpecializationConfiguration : IEntityTypeConfiguration<LawyerSpecialization>
{
    public void Configure(EntityTypeBuilder<LawyerSpecialization> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasColumnType("varchar")
            .HasMaxLength(200);

        builder.Property(s => s.ArabicTitle)
            .IsRequired()
            .HasColumnType("nvarchar")
            .HasMaxLength(200);
    }
}
