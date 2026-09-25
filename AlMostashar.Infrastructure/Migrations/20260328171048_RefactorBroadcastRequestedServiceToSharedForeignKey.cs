using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorBroadcastRequestedServiceToSharedForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (
                    SELECT 1
                    FROM [ClientRequests]
                    WHERE [RequestType] = N'BroadcastRequest'
                      AND [LegalServiceId] IS NOT NULL
                      AND [LawyerServiceLegalServiceId] IS NOT NULL
                      AND [LegalServiceId] <> [LawyerServiceLegalServiceId]
                )
                BEGIN
                    THROW 51000, 'Cannot drop BroadcastRequest.LegalServiceId because some legacy broadcast rows contain conflicting requested-service values.', 1;
                END
                """);

            migrationBuilder.Sql("""
                UPDATE [ClientRequests]
                SET [LawyerServiceLegalServiceId] = [LegalServiceId]
                WHERE [RequestType] = N'BroadcastRequest'
                  AND [LawyerServiceLegalServiceId] IS NULL
                  AND [LegalServiceId] IS NOT NULL;
                """);

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LegalServices_LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_LawyerServiceLegalServiceId",
                table: "ClientRequests",
                column: "LawyerServiceLegalServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_LegalServices_LawyerServiceLegalServiceId",
                table: "ClientRequests",
                column: "LawyerServiceLegalServiceId",
                principalTable: "LegalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LegalServices_LawyerServiceLegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_LawyerServiceLegalServiceId",
                table: "ClientRequests");

            migrationBuilder.AddColumn<int>(
                name: "LegalServiceId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE [ClientRequests]
                SET [LegalServiceId] = [LawyerServiceLegalServiceId]
                WHERE [RequestType] = N'BroadcastRequest'
                  AND [LawyerServiceLegalServiceId] IS NOT NULL;
                """);

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
    }
}
