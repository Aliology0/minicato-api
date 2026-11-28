using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public sealed class LawyerProfileChangeConfiguration : IEntityTypeConfiguration<LawyerProfileChange>
{
    public void Configure(EntityTypeBuilder<LawyerProfileChange> builder)
    {
        builder.ToTable("LawyerProfileChanges");
        builder.HasKey(change => change.Id);
        builder.HasIndex(change => change.LawyerId).IsUnique();
        builder.Property(change => change.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(change => change.LastName).IsRequired().HasMaxLength(100);
        builder.Property(change => change.AvatarUrl).HasMaxLength(500);
        builder.Property(change => change.SSN_Url).HasMaxLength(500);
        builder.Property(change => change.SyndicateCardUrl).HasMaxLength(500);
        builder.Property(change => change.PhoneNo).IsRequired().HasMaxLength(20);
        builder.Property(change => change.Governorate).IsRequired().HasMaxLength(100);
        builder.Property(change => change.City).IsRequired().HasMaxLength(100);
        builder.Property(change => change.Bio).HasMaxLength(1000);
        builder.Property(change => change.About).HasMaxLength(2000);
        builder.Property(change => change.SpecializationIdsJson).IsRequired();
    }
}
