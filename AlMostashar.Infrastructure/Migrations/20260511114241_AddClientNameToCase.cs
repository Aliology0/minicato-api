using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddClientNameToCase : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ClientName",
                table: "Cases",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "ClientName",
                table: "Cases");

        }
    }
}
