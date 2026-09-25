using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRequestLocationLookups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "ClientRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CityId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "GovernorateId",
                table: "ClientRequests",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Governorates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Governorates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Cities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GovernorateId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Cities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Cities_Governorates_GovernorateId",
                        column: x => x.GovernorateId,
                        principalTable: "Governorates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Governorates",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Cairo" },
                    { 2, "Giza" },
                    { 3, "Alexandria" },
                    { 4, "Dakahlia" },
                    { 5, "Red Sea" },
                    { 6, "Beheira" },
                    { 7, "Fayoum" },
                    { 8, "Gharbia" },
                    { 9, "Ismailia" },
                    { 10, "Menofia" },
                    { 11, "Minya" },
                    { 12, "Qalyubia" },
                    { 13, "New Valley" },
                    { 14, "Suez" },
                    { 15, "Aswan" },
                    { 16, "Assiut" },
                    { 17, "Beni Suef" },
                    { 18, "Port Said" },
                    { 19, "Damietta" },
                    { 20, "Sharkia" },
                    { 21, "South Sinai" },
                    { 22, "Kafr El Sheikh" },
                    { 23, "Matrouh" },
                    { 24, "Luxor" },
                    { 25, "Qena" },
                    { 26, "North Sinai" },
                    { 27, "Sohag" }
                });

            migrationBuilder.InsertData(
                table: "Cities",
                columns: new[] { "Id", "GovernorateId", "Name" },
                values: new object[,]
                {
                    { 101, 1, "Cairo" },
                    { 102, 1, "Nasr City" },
                    { 103, 1, "Heliopolis" },
                    { 104, 1, "Maadi" },
                    { 105, 1, "New Cairo" },
                    { 201, 2, "Giza" },
                    { 202, 2, "Dokki" },
                    { 203, 2, "Mohandessin" },
                    { 204, 2, "6th of October" },
                    { 205, 2, "Sheikh Zayed" },
                    { 301, 3, "Alexandria" },
                    { 302, 3, "Smouha" },
                    { 303, 3, "Miami" },
                    { 304, 3, "Borg El Arab" },
                    { 401, 4, "Mansoura" },
                    { 501, 5, "Hurghada" },
                    { 601, 6, "Damanhur" },
                    { 701, 7, "Fayoum" },
                    { 801, 8, "Tanta" },
                    { 901, 9, "Ismailia" },
                    { 1001, 10, "Shibin El Kom" },
                    { 1101, 11, "Minya" },
                    { 1201, 12, "Benha" },
                    { 1301, 13, "Kharga" },
                    { 1401, 14, "Suez" },
                    { 1501, 15, "Aswan" },
                    { 1601, 16, "Assiut" },
                    { 1701, 17, "Beni Suef" },
                    { 1801, 18, "Port Said" },
                    { 1901, 19, "Damietta" },
                    { 2001, 20, "Zagazig" },
                    { 2101, 21, "Sharm El Sheikh" },
                    { 2201, 22, "Kafr El Sheikh" },
                    { 2301, 23, "Marsa Matrouh" },
                    { 2401, 24, "Luxor" },
                    { 2501, 25, "Qena" },
                    { 2601, 26, "Arish" },
                    { 2701, 27, "Sohag" }
                });

            migrationBuilder.Sql(
                """
                UPDATE cr
                SET
                    cr.GovernorateId = g.Id,
                    cr.Governorate = g.Name
                FROM ClientRequests AS cr
                INNER JOIN Governorates AS g
                    ON UPPER(LTRIM(RTRIM(ISNULL(cr.Governorate, '')))) = UPPER(g.Name)
                WHERE cr.GovernorateId IS NULL
                  AND LTRIM(RTRIM(ISNULL(cr.Governorate, ''))) <> '';
                """);

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_CityId",
                table: "ClientRequests",
                column: "CityId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientRequests_GovernorateId",
                table: "ClientRequests",
                column: "GovernorateId");

            migrationBuilder.CreateIndex(
                name: "IX_Cities_GovernorateId_Name",
                table: "Cities",
                columns: new[] { "GovernorateId", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Governorates_Name",
                table: "Governorates",
                column: "Name",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests",
                column: "CityId",
                principalTable: "Cities",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ClientRequests_Governorates_GovernorateId",
                table: "ClientRequests",
                column: "GovernorateId",
                principalTable: "Governorates",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_Cities_CityId",
                table: "ClientRequests");

            migrationBuilder.DropForeignKey(
                name: "FK_ClientRequests_Governorates_GovernorateId",
                table: "ClientRequests");

            migrationBuilder.DropTable(
                name: "Cities");

            migrationBuilder.DropTable(
                name: "Governorates");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_CityId",
                table: "ClientRequests");

            migrationBuilder.DropIndex(
                name: "IX_ClientRequests_GovernorateId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "City",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "CityId",
                table: "ClientRequests");

            migrationBuilder.DropColumn(
                name: "GovernorateId",
                table: "ClientRequests");
        }
    }
}
