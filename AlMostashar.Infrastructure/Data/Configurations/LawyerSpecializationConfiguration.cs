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

        // Seed 19 predefined specializations
        builder.HasData(
            // Islamic Family Law (1-3)
            new LawyerSpecialization { Id = 1, Title = "Islamic Family Law Attorney", ArabicTitle = "محامي الأحوال الشخصية الإسلامية" },
            new LawyerSpecialization { Id = 2, Title = "Non-Muslim Family Law Attorney", ArabicTitle = "محامي الأحوال الشخصية غير الإسلامية" },
            new LawyerSpecialization { Id = 3, Title = "Inheritance and Estate Attorney", ArabicTitle = "محامي الوصايا والتركات" },

            // Criminal Law (4-7)
            new LawyerSpecialization { Id = 4, Title = "Criminal Law Attorney", ArabicTitle = "محامي القانون الجنائي" },
            new LawyerSpecialization { Id = 5, Title = "Public Funds & Illicit Gains Attorney", ArabicTitle = "محامي الأموال العامة والمكاسب غير المشروعة" },
            new LawyerSpecialization { Id = 6, Title = "Narcotics and Weapons Attorney", ArabicTitle = "محامي المخدرات والأسلحة" },
            new LawyerSpecialization { Id = 7, Title = "Cybercrime Attorney", ArabicTitle = "محامي جرائم الإنترنت" },

            // Civil / Property Law (8-9)
            new LawyerSpecialization { Id = 8, Title = "Civil Law Attorney (Contracts & Compensation)", ArabicTitle = "محامي القانون المدني (العقود والتعويضات)" },
            new LawyerSpecialization { Id = 9, Title = "Real Estate and Property Attorney", ArabicTitle = "محامي العقارات والملكية" },

            // Corporate / Economic Law (10-12)
            new LawyerSpecialization { Id = 10, Title = "Corporate Formation Attorney", ArabicTitle = "محامي تأسيس الشركات" },
            new LawyerSpecialization { Id = 11, Title = "Economic Courts Attorney", ArabicTitle = "محامي المحاكم الاقتصادية" },
            new LawyerSpecialization { Id = 12, Title = "Capital Markets & Stock Exchange Attorney", ArabicTitle = "محامي أسواق رأس المال والبورصة" },

            // Administrative / Military Law (13-14)
            new LawyerSpecialization { Id = 13, Title = "Administrative Law Attorney (State Council)", ArabicTitle = "محامي القانون الإداري (مجلس الدولة)" },
            new LawyerSpecialization { Id = 14, Title = "Military & Police Affairs Attorney", ArabicTitle = "محامي الشؤون العسكرية والشرطة" },

            // Intellectual Property (15)
            new LawyerSpecialization { Id = 15, Title = "Intellectual Property Attorney", ArabicTitle = "محامي الملكية الفكرية" },

            // Labor / Social Insurance (16-17)
            new LawyerSpecialization { Id = 16, Title = "Labor & Employment Attorney", ArabicTitle = "محامي العمل والتوظيف" },
            new LawyerSpecialization { Id = 17, Title = "Social Insurance & Pensions Attorney", ArabicTitle = "محامي التأمين الاجتماعي والمعاشات" },

            // International / Immigration Law (18-19)
            new LawyerSpecialization { Id = 18, Title = "Foreigners Affairs & Immigration Attorney", ArabicTitle = "محامي شؤون الأجانب والهجرة" },
            new LawyerSpecialization { Id = 19, Title = "International Law & Arbitration Attorney", ArabicTitle = "محامي القانون الدولي والتحكيم" }
        );
    }
}
