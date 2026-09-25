using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLawyerSpecialization : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LawyerSpecializations",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    ArabicTitle = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerSpecializations", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LawyerSpecializationsMapping",
                columns: table => new
                {
                    LawyerSpecializationsId = table.Column<int>(type: "int", nullable: false),
                    LawyersId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LawyerSpecializationsMapping", x => new { x.LawyerSpecializationsId, x.LawyersId });
                    table.ForeignKey(
                        name: "FK_LawyerSpecializationsMapping_LawyerSpecializations_LawyerSpecializationsId",
                        column: x => x.LawyerSpecializationsId,
                        principalTable: "LawyerSpecializations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LawyerSpecializationsMapping_Users_LawyersId",
                        column: x => x.LawyersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LawyerSpecializationsMapping_LawyersId",
                table: "LawyerSpecializationsMapping",
                column: "LawyersId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "LawyerSpecializationsMapping");

            migrationBuilder.DropTable(
                name: "LawyerSpecializations");
        }
    }
}
