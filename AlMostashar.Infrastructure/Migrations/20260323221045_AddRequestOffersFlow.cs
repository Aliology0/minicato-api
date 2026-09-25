using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestOffersFlow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RequestOffers",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientRequestId = table.Column<int>(type: "int", nullable: false),
                    LawyerId = table.Column<int>(type: "int", nullable: false),
                    LegalServiceId = table.Column<int>(type: "int", nullable: false),
                    OfferedAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Note = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    RespondedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RequestOffers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RequestOffers_ClientRequests_ClientRequestId",
                        column: x => x.ClientRequestId,
                        principalTable: "ClientRequests",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RequestOffers_LegalServices_LegalServiceId",
                        column: x => x.LegalServiceId,
                        principalTable: "LegalServices",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RequestOffers_Users_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RequestOffers_LawyerId",
                table: "RequestOffers",
                column: "LawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestOffers_LegalServiceId",
                table: "RequestOffers",
                column: "LegalServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_RequestOffers_Status",
                table: "RequestOffers",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "UX_RequestOffers_PendingByRequestLawyer",
                table: "RequestOffers",
                columns: new[] { "ClientRequestId", "LawyerId" },
                unique: true,
                filter: "[Status] = 'Pending'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RequestOffers");
        }
    }
}
