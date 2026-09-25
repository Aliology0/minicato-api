using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HardenWalletWithdrawals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AccountDetailsEncrypted",
                table: "WithdrawalRequests",
                type: "nvarchar(2048)",
                maxLength: 2048,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "AccountDetailsMasked",
                table: "WithdrawalRequests",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql("""
                UPDATE [WithdrawalRequests]
                SET [AccountDetailsMasked] =
                    CASE
                        WHEN [AccountDetails] IS NULL OR LEN([AccountDetails]) = 0 THEN ''
                        WHEN LEN([AccountDetails]) <= 4 THEN REPLICATE('*', LEN([AccountDetails]))
                        ELSE CONCAT(
                            REPLICATE('*', CASE WHEN LEN([AccountDetails]) - 4 > 8 THEN 8 ELSE LEN([AccountDetails]) - 4 END),
                            RIGHT([AccountDetails], 4))
                    END
                """);

            migrationBuilder.DropColumn(
                name: "AccountDetails",
                table: "WithdrawalRequests");

            migrationBuilder.AddColumn<int>(
                name: "PaidByAdminId",
                table: "WithdrawalRequests",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutProvider",
                table: "WithdrawalRequests",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PayoutReference",
                table: "WithdrawalRequests",
                type: "nvarchar(300)",
                maxLength: 300,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_PaidAt",
                table: "WithdrawalRequests",
                column: "PaidAt");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_PayoutReference",
                table: "WithdrawalRequests",
                column: "PayoutReference",
                unique: true,
                filter: "[PayoutReference] IS NOT NULL");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_WalletId_Status",
                table: "WithdrawalRequests",
                columns: new[] { "WalletId", "Status" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_PaidAt",
                table: "WithdrawalRequests");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_PayoutReference",
                table: "WithdrawalRequests");

            migrationBuilder.DropIndex(
                name: "IX_WithdrawalRequests_WalletId_Status",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "AccountDetailsEncrypted",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "AccountDetailsMasked",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "PaidByAdminId",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "PayoutProvider",
                table: "WithdrawalRequests");

            migrationBuilder.DropColumn(
                name: "PayoutReference",
                table: "WithdrawalRequests");

            migrationBuilder.AddColumn<string>(
                name: "AccountDetails",
                table: "WithdrawalRequests",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                defaultValue: "");
        }
    }
}
