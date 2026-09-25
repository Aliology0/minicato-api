using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddAcceptedOfferUniqueInvariant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "UX_RequestOffers_AcceptedPerRequest",
                table: "RequestOffers",
                column: "ClientRequestId",
                unique: true,
                filter: "[Status] = 'Accepted'");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "UX_RequestOffers_AcceptedPerRequest",
                table: "RequestOffers");
        }
    }
}
