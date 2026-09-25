using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLegalServicesAndCasesTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LawyerServices_LawyerServiceLawyerId_LawyerServiceServiceId",
                table: "ClientRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerServices_Services_ServiceId",
                table: "LawyerServices");

            migrationBuilder.DropTable(
                name: "Services");

            migrationBuilder.RenameColumn(
                name: "ReferencesId",
                table: "WalletTransactions",
                newName: "ReferenceId");

            migrationBuilder.RenameColumn(
                name: "isEmailVerified",
                table: "Users",
                newName: "IsEmailVerified");

            migrationBuilder.RenameColumn(
                name: "ServiceId",
                table: "LawyerServices",
                newName: "LegalServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_LawyerServices_ServiceId",
                table: "LawyerServices",
                newName: "IX_LawyerServices_LegalServiceId");

            migrationBuilder.RenameColumn(
                name: "LawyerServiceServiceId",
                table: "ClientRequests",
                newName: "LawyerServiceLegalServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientRequests_LawyerServiceLawyerId_LawyerServiceServiceId",
                table: "ClientRequests",
                newName: "IX_ClientRequests_LawyerServiceLawyerId_LawyerServiceLegalServiceId");

            migrationBuilder.RenameColumn(
                name: "sentAt",
                table: "ChatMessages",
                newName: "SentAt");

            migrationBuilder.RenameColumn(
                name: "title",
                table: "CaseTimelines",
                newName: "Title");

            migrationBuilder.RenameColumn(
                name: "content",
                table: "CaseTimelines",
                newName: "Content");

            migrationBuilder.AddColumn<int>(
                name: "AccountStatus",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ChatMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                table: "ChatMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "CourtName",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "CaseNumber",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<int>(
                name: "AllowedRevisions",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalAmount",
                table: "Cases",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRole",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommercialRegistrationNo",
                table: "Cases",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommunicationMethod",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ConsultationSummary",
                table: "Cases",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractType",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeliveryDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundersCount",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPowerOfAttorney",
                table: "Cases",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LawsuitStatus",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalBranch",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextHearingDate",
                table: "Cases",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ServiceType",
                table: "Cases",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "UsedRevisions",
                table: "Cases",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LegalServices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    ServiceType = table.Column<string>(type: "nvarchar(21)", maxLength: 21, nullable: false),
                    CompanyType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CapitalAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    FoundersCount = table.Column<int>(type: "int", nullable: true),
                    HasPowerOfAttorney = table.Column<bool>(type: "bit", nullable: true),
                    AppointmentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CommunicationMethod = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LegalBranch = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    ContractType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Language = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    AllowedRevisions = table.Column<int>(type: "int", nullable: true),
                    CourtName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CaseNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    NextHearingDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CurrentStatus = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ClientRole = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LegalServices", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_LawyerServices_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "ClientRequests",
                columns: new[] { "LawyerServiceLawyerId", "LawyerServiceLegalServiceId" },
                principalTable: "LawyerServices",
                principalColumns: new[] { "LawyerId", "LegalServiceId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerServices_LegalServices_LegalServiceId",
                table: "LawyerServices",
                column: "LegalServiceId",
                principalTable: "LegalServices",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_LawyerServices_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "ClientRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_LawyerServices_LegalServices_LegalServiceId",
                table: "LawyerServices");

            migrationBuilder.DropTable(
                name: "LegalServices");

            migrationBuilder.DropColumn(
                name: "AccountStatus",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "AllowedRevisions",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CapitalAmount",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ClientRole",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CommercialRegistrationNo",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CommunicationMethod",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ConsultationSummary",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "DeliveryDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "FoundersCount",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "HasPowerOfAttorney",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LawsuitStatus",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "LegalBranch",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "NextHearingDate",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "ServiceType",
                table: "Cases");

            migrationBuilder.DropColumn(
                name: "UsedRevisions",
                table: "Cases");

            migrationBuilder.RenameColumn(
                name: "ReferenceId",
                table: "WalletTransactions",
                newName: "ReferencesId");

            migrationBuilder.RenameColumn(
                name: "IsEmailVerified",
                table: "Users",
                newName: "isEmailVerified");

            migrationBuilder.RenameColumn(
                name: "LegalServiceId",
                table: "LawyerServices",
                newName: "ServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_LawyerServices_LegalServiceId",
                table: "LawyerServices",
                newName: "IX_LawyerServices_ServiceId");

            migrationBuilder.RenameColumn(
                name: "LawyerServiceLegalServiceId",
                table: "ClientRequests",
                newName: "LawyerServiceServiceId");

            migrationBuilder.RenameIndex(
                name: "IX_ClientRequests_LawyerServiceLawyerId_LawyerServiceLegalServiceId",
                table: "ClientRequests",
                newName: "IX_ClientRequests_LawyerServiceLawyerId_LawyerServiceServiceId");

            migrationBuilder.RenameColumn(
                name: "SentAt",
                table: "ChatMessages",
                newName: "sentAt");

            migrationBuilder.RenameColumn(
                name: "Title",
                table: "CaseTimelines",
                newName: "title");

            migrationBuilder.RenameColumn(
                name: "Content",
                table: "CaseTimelines",
                newName: "content");

            migrationBuilder.AlterColumn<string>(
                name: "FileName",
                table: "ChatMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AttachmentUrl",
                table: "ChatMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CourtName",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "CaseNumber",
                table: "Cases",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Details = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    Name = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_LawyerServices_LawyerServiceLawyerId_LawyerServiceServiceId",
                table: "ClientRequests",
                columns: new[] { "LawyerServiceLawyerId", "LawyerServiceServiceId" },
                principalTable: "LawyerServices",
                principalColumns: new[] { "LawyerId", "ServiceId" },
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LawyerServices_Services_ServiceId",
                table: "LawyerServices",
                column: "ServiceId",
                principalTable: "Services",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
