using AlMostashar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AlMostashar.Infrastructure.Data.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        // ─── TPH Discriminator (User ← Admin, Client, Lawyer) ───
        builder.HasDiscriminator<string>("UserType")
            .HasValue<Admin>("Admin")
            .HasValue<Client>("Client")
            .HasValue<Lawyer>("Lawyer");

        // ─── Table ───
        builder.ToTable("Users");

        // ─── Properties — IdentityUser (base) ───
        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256);

        builder.Property(u=> u.AccountStatus)
        .HasConversion<string>()
        .HasColumnType("varchar")
        .HasMaxLength(50);

        builder.Property(u => u.VerificationStatus)
            .HasConversion<string>()
            .HasColumnType("varchar")
            .HasMaxLength(50)
            .HasDefaultValue(AlMostashar.Domain.ValueObject.Enum.VerificationStatus.Pending);


        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired()
            .HasMaxLength(512);

        builder.Property(u => u.OTPcode)
            .HasMaxLength(10);

        // ─── Properties — User ───
        builder.Property(u => u.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.FullName)
            .HasMaxLength(200);

        builder.Property(u => u.AvatarUrl)
            .HasMaxLength(500);

        // ─── Relationships ───

        // 1:N — User → Notification
        builder.HasMany(u => u.Notifications)
            .WithOne(n => n.User)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // 1:N — User → ChatParticipant
        builder.HasMany(u => u.ChatParticipants)
            .WithOne(cp => cp.User)
            .HasForeignKey(cp => cp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
