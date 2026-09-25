using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupportRealtimeCalling : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CallSessions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChannelName = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    CallType = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    Status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DurationInSeconds = table.Column<int>(type: "int", nullable: true),
                    ChatId = table.Column<int>(type: "int", nullable: false),
                    CallerId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallSessions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CallSessions_Chats_ChatId",
                        column: x => x.ChatId,
                        principalTable: "Chats",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_CallerId",
                table: "CallSessions",
                column: "CallerId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChannelName",
                table: "CallSessions",
                column: "ChannelName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChatId",
                table: "CallSessions",
                column: "ChatId");

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ChatId_Status",
                table: "CallSessions",
                columns: new[] { "ChatId", "Status" });

            migrationBuilder.CreateIndex(
                name: "IX_CallSessions_ReceiverId",
                table: "CallSessions",
                column: "ReceiverId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CallSessions");
        }
    }
}
