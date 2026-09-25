using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateChatEntitiesAndLinkMessagesToDocuments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrl",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "FileName",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastMessageAt",
                table: "Chats",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastMessageContent",
                table: "Chats",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "UnReadMessageCount",
                table: "ChatParticipants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "CaseDocumentId",
                table: "ChatMessages",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_LastMessageAt",
                table: "Chats",
                column: "LastMessageAt",
                descending: new bool[0]);

            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_CaseDocumentId",
                table: "ChatMessages",
                column: "CaseDocumentId",
                unique: true,
                filter: "[CaseDocumentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatMessages_CaseDocuments_CaseDocumentId",
                table: "ChatMessages",
                column: "CaseDocumentId",
                principalTable: "CaseDocuments",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatMessages_CaseDocuments_CaseDocumentId",
                table: "ChatMessages");

            migrationBuilder.DropIndex(
                name: "IX_Chats_LastMessageAt",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_CaseDocumentId",
                table: "ChatMessages");

            migrationBuilder.DropColumn(
                name: "LastMessageContent",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "UnReadMessageCount",
                table: "ChatParticipants");

            migrationBuilder.DropColumn(
                name: "CaseDocumentId",
                table: "ChatMessages");

            migrationBuilder.AlterColumn<DateTime>(
                name: "LastMessageAt",
                table: "Chats",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrl",
                table: "ChatMessages",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FileName",
                table: "ChatMessages",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
