using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RedesignClientRequestCaseFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LegalServices_LawyerServiceLegalServiceId",
                table: "ClientRequests");


            migrationBuilder.AddColumn<int>(
                name: "AcceptedOfferId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ClientDeadline",
                table: "ClientRequests",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "LegalServiceId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PreferredCommunicationMethod",
                table: "ClientRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RequestDetailsJson",
                table: "ClientRequests",
                type: "nvarchar(max)",
                maxLength: 8000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ServiceType",
                table: "ClientRequests",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Base");

            migrationBuilder.AddColumn<string>(
                name: "Urgency",
                table: "ClientRequests",
                type: "nvarchar(30)",
                maxLength: 30,
                nullable: false,
                defaultValue: "Normal");

            migrationBuilder.AddColumn<int>(
                name: "UploadedByClientId",
                table: "CaseDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.Sql("""
                UPDATE cr
                SET cr.LegalServiceId = cr.LawyerServiceLegalServiceId,
                    cr.ServiceType = COALESCE(ls.ServiceType, 'Base')
                FROM ClientRequests cr
                LEFT JOIN LegalServices ls ON ls.Id = cr.LawyerServiceLegalServiceId;

                UPDATE cr
                SET cr.AcceptedOfferId = acceptedOffer.Id
                FROM ClientRequests cr
                INNER JOIN RequestOffers acceptedOffer
                    ON acceptedOffer.ClientRequestId = cr.Id AND acceptedOffer.Status = 'Accepted';
                """);


            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_CreatedAt",
                table: "ClientRequests",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_LegalServiceId",
                table: "ClientRequests",
                column: "LegalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_ServiceType",
                table: "ClientRequests",
                column: "ServiceType");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_Status",
                table: "ClientRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_UploadedByClientId",
                table: "CaseDocuments",
                column: "UploadedByClientId");

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
                name: "IX_ClientRequests_CreatedAt",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_ServiceType",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_Status",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_UploadedByClientId",
                table: "CaseDocuments");

            migrationBuilder.DropColumn(
                name: "AcceptedLawyerId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "AcceptedOfferId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "ClientDeadline",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "LegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "PreferredCommunicationMethod",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "RequestDetailsJson",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "TargetLawyerId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "Urgency",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "UploadedByClientId",
                table: "CaseDocuments");

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_LegalServices_LawyerServiceLegalServiceId",
                table: "ClientRequests",
                column: "LawyerServiceLegalServiceId",
                principalTable: "LegalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
