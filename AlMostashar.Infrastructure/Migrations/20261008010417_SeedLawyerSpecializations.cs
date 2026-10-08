using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedLawyerSpecializations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "LawyerSpecializations",
                columns: new[] { "Id", "ArabicTitle", "Title" },
                values: new object[,]
                {
                    { 1, "محامي الأحوال الشخصية الإسلامية", "Islamic Family Law Attorney" },
                    { 2, "محامي الأحوال الشخصية غير الإسلامية", "Non-Muslim Family Law Attorney" },
                    { 3, "محامي الوصايا والتركات", "Inheritance and Estate Attorney" },
                    { 4, "محامي القانون الجنائي", "Criminal Law Attorney" },
                    { 5, "محامي الأموال العامة والمكاسب غير المشروعة", "Public Funds & Illicit Gains Attorney" },
                    { 6, "محامي المخدرات والأسلحة", "Narcotics and Weapons Attorney" },
                    { 7, "محامي جرائم الإنترنت", "Cybercrime Attorney" },
                    { 8, "محامي القانون المدني (العقود والتعويضات)", "Civil Law Attorney (Contracts & Compensation)" },
                    { 9, "محامي العقارات والملكية", "Real Estate and Property Attorney" },
                    { 10, "محامي تأسيس الشركات", "Corporate Formation Attorney" },
                    { 11, "محامي المحاكم الاقتصادية", "Economic Courts Attorney" },
                    { 12, "محامي أسواق رأس المال والبورصة", "Capital Markets & Stock Exchange Attorney" },
                    { 13, "محامي القانون الإداري (مجلس الدولة)", "Administrative Law Attorney (State Council)" },
                    { 14, "محامي الشؤون العسكرية والشرطة", "Military & Police Affairs Attorney" },
                    { 15, "محامي الملكية الفكرية", "Intellectual Property Attorney" },
                    { 16, "محامي العمل والتوظيف", "Labor & Employment Attorney" },
                    { 17, "محامي التأمين الاجتماعي والمعاشات", "Social Insurance & Pensions Attorney" },
                    { 18, "محامي شؤون الأجانب والهجرة", "Foreigners Affairs & Immigration Attorney" },
                    { 19, "محامي القانون الدولي والتحكيم", "International Law & Arbitration Attorney" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "LawyerSpecializations",
                keyColumn: "Id",
                keyValue: 19);
        }
    }
}
