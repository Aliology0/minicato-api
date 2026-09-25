using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddFeedbackRelationAndUserDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "LawyerServiceLawyerId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LawyerServiceLegalServiceId",
                table: "Feedbacks",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Feedbacks_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "Feedbacks",
                columns: new[] { "LawyerServiceLawyerId", "LawyerServiceLegalServiceId" });

            migrationBuilder.AddForeignKey(
                name: "FK_Feedbacks_LawyerServices_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "Feedbacks",
                columns: new[] { "LawyerServiceLawyerId", "LawyerServiceLegalServiceId" },
                principalTable: "LawyerServices",
                principalColumns: new[] { "LawyerId", "LegalServiceId" },
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Feedbacks_LawyerServices_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "Feedbacks");

            migrationBuilder.DropIndex(
                name: "IX_Feedbacks_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LawyerServiceLawyerId",
                table: "Feedbacks");

            migrationBuilder.DropColumn(
                name: "LawyerServiceLegalServiceId",
                table: "Feedbacks");
        }
    }
}
