using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueConstraintToActiveCallSessions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallSessions_ChatId",
                table: "CallSessions");

            migrationBuilder.AlterColumn<string>(
                name: "ClientName",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChatId_ActiveCall",
                table: "CallSessions",
                column: "ChatId",
                unique: true,
                filter: "[Status] IN ('Initiated', 'Ongoing')");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_CallSessions_ChatId_ActiveCall",
                table: "CallSessions");

            migrationBuilder.AlterColumn<string>(
                name: "ClientName",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChatId",
                table: "CallSessions",
                column: "ChatId");
        }
    }
}
