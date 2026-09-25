using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestGovernorateAndBroadcastLegalService : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Governorate",
                table: "ClientRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "LegalServiceId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_LegalServiceId",
                table: "ClientRequests",
                column: "LegalServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_LegalServices_LegalServiceId",
                table: "ClientRequests",
                column: "LegalServiceId",
                principalTable: "LegalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LegalServices_LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "Governorate",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "LegalServiceId",
                table: "ClientRequests");
        }
    }
}
