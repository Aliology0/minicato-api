using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        // ─── Relationships ───

        // 1:N — Client → ClientRequest
        builder.HasMany(c => c.ClientRequests)
            .WithOne(cr => cr.Client)
            .HasForeignKey(cr => cr.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Property(c => c.Governorate)
            .HasMaxLength(100);

        builder.Property(c => c.City)
            .HasMaxLength(100);
    }
}
