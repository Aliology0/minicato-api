using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMarkAsReadCompositeIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_ChatMessages_MarkAsReadCovering",
                table: "ChatMessages",
                columns: new[] { "ChatId", "IsRead", "SenderId", "Id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ChatMessages_MarkAsReadCovering",
                table: "ChatMessages");
        }
    }
}
