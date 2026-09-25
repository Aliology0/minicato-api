using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SecureReportDocumentAttachments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments");

            migrationBuilder.AddColumn<int>(
                name: "ReportId",
                table: "CaseDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_ReportId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments",
                columns: new[] { "ClientRequestId", "CaseId", "ReportId", "CreatedAt", "CleanupClaimToken" });

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_ReportId",
                table: "CaseDocuments",
                column: "ReportId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseDocuments_Reports_ReportId",
                table: "CaseDocuments",
                column: "ReportId",
                principalTable: "Reports",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseDocuments_Reports_ReportId",
                table: "CaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_ReportId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_ReportId",
                table: "CaseDocuments");

            migrationBuilder.DropColumn(
                name: "ReportId",
                table: "CaseDocuments");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_ClientRequestId_CaseId_CreatedAt_CleanupClaimToken",
                table: "CaseDocuments",
                columns: new[] { "ClientRequestId", "CaseId", "CreatedAt", "CleanupClaimToken" });
        }
    }
}
