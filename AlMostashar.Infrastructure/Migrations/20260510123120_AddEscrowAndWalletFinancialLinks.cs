using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AlMostashar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEscrowAndWalletFinancialLinks : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escrows_ClientRequests_RequestId",
                table: "Escrows");

            migrationBuilder.AddColumn<int>(
                name: "EscrowId",
                table: "WalletTransactions",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EscrowDisputedAt",
                table: "Escrows",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaymentId",
                table: "Escrows",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions",
                column: "EscrowId");

            migrationBuilder.CreateIndex(
                name: "IX_Escrows_PaymentId",
                table: "Escrows",
                column: "PaymentId",
                unique: true,
                filter: "[PaymentId] IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_Escrows_ClientRequests_RequestId",
                table: "Escrows",
                column: "RequestId",
                principalTable: "ClientRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Escrows_Payments_PaymentId",
                table: "Escrows",
                column: "PaymentId",
                principalTable: "Payments",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_WalletTransactions_Escrows_EscrowId",
                table: "WalletTransactions",
                column: "EscrowId",
                principalTable: "Escrows",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Escrows_ClientRequests_RequestId",
                table: "Escrows");

            migrationBuilder.DropForeignKey(
                name: "FK_Escrows_Payments_PaymentId",
                table: "Escrows");

            migrationBuilder.DropForeignKey(
                name: "FK_WalletTransactions_Escrows_EscrowId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_WalletTransactions_EscrowId",
                table: "WalletTransactions");

            migrationBuilder.DropIndex(
                name: "IX_Escrows_PaymentId",
                table: "Escrows");

            migrationBuilder.DropColumn(
                name: "EscrowId",
                table: "WalletTransactions");

            migrationBuilder.DropColumn(
                name: "EscrowDisputedAt",
                table: "Escrows");

            migrationBuilder.DropColumn(
                name: "PaymentId",
                table: "Escrows");

            migrationBuilder.AddForeignKey(
                name: "FK_Escrows_ClientRequests_RequestId",
                table: "Escrows",
                column: "RequestId",
                principalTable: "ClientRequests",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
