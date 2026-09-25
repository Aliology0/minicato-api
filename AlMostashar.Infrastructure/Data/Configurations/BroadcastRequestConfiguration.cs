using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class BroadcastRequestConfiguration : IEntityTypeConfiguration<BroadcastRequest>
{
    public void Configure(EntityTypeBuilder<BroadcastRequest> builder)
    {
        // ─── Properties ───
        builder.Property(br => br.Budget)
            .IsRequired()
            .HasColumnType("decimal(18,2)");
    }
}
