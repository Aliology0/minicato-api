using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenDocumentCleanupAndNullableConsultationAppointment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CleanupClaimToken",
                table: "CaseDocuments",
                type: "nvarchar(36)",
                maxLength: 36,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CleanupClaimedAt",
                table: "CaseDocuments",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments",
                columns: new[] { "ClientRequestId", "CaseId", "CreatedAt", "CleanupClaimToken" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments");

            migrationBuilder.DropColumn(
                name: "CleanupClaimToken",
                table: "CaseDocuments");

            migrationBuilder.DropColumn(
                name: "CleanupClaimedAt",
                table: "CaseDocuments");
        }
    }
}
