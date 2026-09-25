using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientRequestDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseDocuments_Cases_CaseId",
                table: "CaseDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "CaseId",
                table: "CaseDocuments",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ClientRequestId",
                table: "CaseDocuments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CaseDocuments_ClientRequestId",
                table: "CaseDocuments",
                column: "ClientRequestId");

            migrationBuilder.AddForeignKey(
                name: "FK_CaseDocuments_Cases_CaseId",
                table: "CaseDocuments",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseDocuments_ClientRequests_ClientRequestId",
                table: "CaseDocuments",
                column: "ClientRequestId",
                principalTable: "ClientRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CaseDocuments_Cases_CaseId",
                table: "CaseDocuments");

            migrationBuilder.DropForeignKey(
                name: "FK_CaseDocuments_ClientRequests_ClientRequestId",
                table: "CaseDocuments");

            migrationBuilder.DropIndex(
                name: "IX_CaseDocuments_ClientRequestId",
                table: "CaseDocuments");

            migrationBuilder.DropColumn(
                name: "ClientRequestId",
                table: "CaseDocuments");

            migrationBuilder.AlterColumn<int>(
                name: "CaseId",
                table: "CaseDocuments",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CaseDocuments_Cases_CaseId",
                table: "CaseDocuments",
                column: "CaseId",
                principalTable: "Cases",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
