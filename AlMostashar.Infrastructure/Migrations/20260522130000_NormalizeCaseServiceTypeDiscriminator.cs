using AlMostashar.Infrastructure.Data;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <summary>
    /// Repairs legacy case discriminator values stored as strings before
    /// the application standardized on enum-backed service types.
    /// </summary>
    [DbContext(typeof(AlmostasharDbContext))]
    [Migration("20260522130000_NormalizeCaseServiceTypeDiscriminator")]
    public partial class NormalizeCaseServiceTypeDiscriminator : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM sys.columns c
                    INNER JOIN sys.types t ON c.user_type_id = t.user_type_id
                    WHERE c.object_id = OBJECT_ID(N'[Cases]')
                      AND c.name = N'ServiceType'
                      AND t.name IN (N'varchar', N'nvarchar', N'char', N'nchar', N'text', N'ntext')
                )
                BEGIN
                    UPDATE [Cases]
                    SET [ServiceType] = CASE LTRIM(RTRIM(COALESCE([ServiceType], N'')))
                        WHEN N'Consultation' THEN N'0'
                        WHEN N'Contract' THEN N'1'
                        WHEN N'CompanyFormation' THEN N'2'
                        WHEN N'Lawsuit' THEN N'3'
                        WHEN N'Base' THEN N'4'
                        WHEN N'IntellectualProperty' THEN N'5'
                        WHEN N'LegalTranslation' THEN N'6'
                        WHEN N'DueDiligence' THEN N'7'
                        WHEN N'DebtCollection' THEN N'8'
                        WHEN N'Mediation' THEN N'9'
                        WHEN N'LegalReview' THEN N'10'
                        WHEN N'Compliance' THEN N'11'
                        WHEN N'' THEN N'4'
                        ELSE COALESCE(CONVERT(nvarchar(20), TRY_CONVERT(int, [ServiceType])), N'4')
                    END;

                    ALTER TABLE [Cases] ALTER COLUMN [ServiceType] int NOT NULL;
                END
                ELSE
                BEGIN
                    UPDATE [Cases]
                    SET [ServiceType] = 4
                    WHERE [ServiceType] IS NULL;
                END
                """);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Forward-only repair migration.
        }
    }
}
