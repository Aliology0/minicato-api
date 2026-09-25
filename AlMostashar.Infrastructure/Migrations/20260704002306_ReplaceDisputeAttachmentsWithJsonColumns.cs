using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ReplaceDisputeAttachmentsWithJsonColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttachmentUrls",
                table: "Disputes");

            migrationBuilder.AddColumn<string>(
                name: "Attachments",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DisputeChatMessages",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Attachments",
                table: "Disputes");

            migrationBuilder.DropColumn(
                name: "DisputeChatMessages",
                table: "Disputes");

            migrationBuilder.AddColumn<string>(
                name: "AttachmentUrls",
                table: "Disputes",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
