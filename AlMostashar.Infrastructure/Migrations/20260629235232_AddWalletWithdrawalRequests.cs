using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddWalletWithdrawalRequests : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions");

            migrationBuilder.AddColumn<decimal>(
                name: "BalanceAfter",
                table: "WalletTransactions",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CaseId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "WalletTransactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Description",
                table: "WalletTransactions",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "InvoiceId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "WithdrawalRequestId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WithdrawalRequests",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    LawyerId = table.Column<int>(type: "int", nullable: false),
                    WalletId = table.Column<int>(type: "int", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Method = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    AccountDetails = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    RequestedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ReviewedByAdminId = table.Column<int>(type: "int", nullable: true),
                    AdminNotes = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    RejectionReason = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    PaidAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    WalletTransactionId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WithdrawalRequests", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Users_LawyerId",
                        column: x => x.LawyerId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WithdrawalRequests_Wallets_WalletId",
                        column: x => x.WalletId,
                        principalTable: "Wallets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CaseId",
                table: "WalletTransactions",
                column: "CaseId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_CreatedAt",
                table: "WalletTransactions",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions",
                column: "EscrowId",
                unique: true,
                filter: "[EscrowId] IS NOT NULL AND [Type] = 'Credit'");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_InvoiceId",
                table: "WalletTransactions",
                column: "InvoiceId");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_WithdrawalRequestId",
                table: "WalletTransactions",
                column: "WithdrawalRequestId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_LawyerId",
                table: "WithdrawalRequests",
                column: "LawyerId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_RequestedAt",
                table: "WithdrawalRequests",
                column: "RequestedAt");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_Status",
                table: "WithdrawalRequests",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_WalletId",
                table: "WithdrawalRequests",
                column: "WalletId");

            migrationBuilder.CreateIndex(
                name: "IX_WithdrawalRequests_WalletTransactionId",
                table: "WithdrawalRequests",
                column: "WalletTransactionId");

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_WithdrawalRequests_WithdrawalRequestId",
                table: "WalletTransactions",
                column: "WithdrawalRequestId",
                principalTable: "WithdrawalRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_WithdrawalRequests_WithdrawalRequestId",
                table: "WalletTransactions");

            migrationBuilder.DropTable(
                name: "WithdrawalRequests");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_CaseId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_CreatedAt",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_InvoiceId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_WithdrawalRequestId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "BalanceAfter",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CaseId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "Description",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "InvoiceId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "WithdrawalRequestId",
                table: "WalletTransactions");

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions",
                column: "EscrowId");
        }
    }
}
