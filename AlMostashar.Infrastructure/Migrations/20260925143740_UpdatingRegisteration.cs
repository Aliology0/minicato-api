using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdatingRegisteration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "NationalIdPhotoUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "VerificationStatus",
                table: "Users",
                type: "varchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Pending");

            migrationBuilder.CreateIndex(
                name: "IX_Users_SyndicateId",
                table: "Users",
                column: "SyndicateId",
                unique: true,
                filter: "[SyndicateId] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Users_SyndicateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "NationalIdPhotoUrl",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "VerificationStatus",
                table: "Users");
        }
    }
}
