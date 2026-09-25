using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DeleteLegalServiceTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedRevisions",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "AppointmentDate",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CapitalAmount",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CaseNumber",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "ClientRole",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CommunicationMethod",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CompanyType",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "ContractType",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CourtName",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CurrentStatus",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "FoundersCount",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "HasPowerOfAttorney",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "LegalBranch",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "NextHearingDate",
                table: "LegalServices");

            migrationBuilder.RenameColumn(
                name: "Language",
                table: "LegalServices",
                newName: "ExpectedDuration");

            migrationBuilder.RenameColumn(
                name: "Details",
                table: "LegalServices",
                newName: "FullDescription");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "LegalServices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<int>(
                name: "ServiceType",
                table: "LegalServices",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(21)",
                oldMaxLength: 21);

            migrationBuilder.AddColumn<int>(
                name: "AdminId",
                table: "LegalServices",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "LegalServices",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "IconUrl",
                table: "LegalServices",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LegalServices",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "RequiredDocuments",
                table: "LegalServices",
                type: "nvarchar(4000)",
                maxLength: 4000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "LegalServices",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "LawyerServices",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_LegalServices_AdminId",
                table: "LegalServices",
                column: "AdminId");

            migrationBuilder.CreateIndex(
                name: "IX_LegalServices_ServiceType",
                table: "LegalServices",
                column: "ServiceType",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_LegalServices_Users_AdminId",
                table: "LegalServices",
                column: "AdminId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LegalServices_Users_AdminId",
                table: "LegalServices");

            migrationBuilder.DropIndex(
                name: "IX_LegalServices_AdminId",
                table: "LegalServices");

            migrationBuilder.DropIndex(
                name: "IX_LegalServices_ServiceType",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "AdminId",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "IconUrl",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "RequiredDocuments",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "LegalServices");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "LawyerServices");

            migrationBuilder.RenameColumn(
                name: "FullDescription",
                table: "LegalServices",
                newName: "Details");

            migrationBuilder.RenameColumn(
                name: "ExpectedDuration",
                table: "LegalServices",
                newName: "Language");

            migrationBuilder.AlterColumn<string>(
                name: "Summary",
                table: "LegalServices",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(500)",
                oldMaxLength: 500);

            migrationBuilder.AlterColumn<string>(
                name: "ServiceType",
                table: "LegalServices",
                type: "nvarchar(21)",
                maxLength: 21,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "AllowedRevisions",
                table: "LegalServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "AppointmentDate",
                table: "LegalServices",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CapitalAmount",
                table: "LegalServices",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CaseNumber",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClientRole",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommunicationMethod",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CompanyType",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ContractType",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CourtName",
                table: "LegalServices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CurrentStatus",
                table: "LegalServices",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "FoundersCount",
                table: "LegalServices",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "HasPowerOfAttorney",
                table: "LegalServices",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LegalBranch",
                table: "LegalServices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "LegalServices",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "NextHearingDate",
                table: "LegalServices",
                type: "datetime2",
                nullable: true);
        }
    }
}
