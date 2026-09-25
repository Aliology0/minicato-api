using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class AdminConfiguration : IEntityTypeConfiguration<Admin>
{
    public void Configure(EntityTypeBuilder<Admin> builder)
    {
        // ─── Relationships ───

        // 1:N — Admin → Lawyer (Verifies)
        builder.HasMany(a => a.VerifiedLawyers)
            .WithOne(l => l.VerifiedByAdmin)
            .HasForeignKey(l => l.VerifiedByAdminId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
