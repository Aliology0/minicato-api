using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorRegistrationAndUserDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SyndicateMembershipCardUrl",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "Client_City",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Client_CityId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Client_Governorate",
                table: "Users",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Client_GovernorateId",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Client_City",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Client_CityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Client_Governorate",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "Client_GovernorateId",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "SyndicateMembershipCardUrl",
                table: "Users",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);
        }
    }
}
