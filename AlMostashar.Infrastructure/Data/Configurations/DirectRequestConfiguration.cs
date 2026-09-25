using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class DirectRequestConfiguration : IEntityTypeConfiguration<DirectRequest>
{
    public void Configure(EntityTypeBuilder<DirectRequest> builder)
    {
        // No additional properties — DirectRequest inherits all from ClientRequest.
        // Discriminator value is already set in ClientRequestConfiguration.
    }
}
