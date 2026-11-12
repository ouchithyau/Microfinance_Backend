using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseAccountingFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "JournalEntryLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOnUtc",
                table: "JournalEntryLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "JournalEntryLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "JournalEntryLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedByEntryId",
                table: "JournalEntryLines",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ApprovedByUserId",
                table: "JournalEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOnUtc",
                table: "JournalEntries",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "JournalEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "JournalEntries",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ReversedByEntryId",
                table: "JournalEntries",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsSystemControlled",
                table: "GeneralLedgerAccounts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<Guid>(
                name: "ParentAccountId",
                table: "GeneralLedgerAccounts",
                type: "uuid",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_GeneralLedgerAccounts_ParentAccountId",
                table: "GeneralLedgerAccounts",
                column: "ParentAccountId");

            migrationBuilder.AddForeignKey(
                name: "FK_GeneralLedgerAccounts_GeneralLedgerAccounts_ParentAccountId",
                table: "GeneralLedgerAccounts",
                column: "ParentAccountId",
                principalTable: "GeneralLedgerAccounts",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_GeneralLedgerAccounts_GeneralLedgerAccounts_ParentAccountId",
                table: "GeneralLedgerAccounts");

            migrationBuilder.DropIndex(
                name: "IX_GeneralLedgerAccounts_ParentAccountId",
                table: "GeneralLedgerAccounts");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ApprovedOnUtc",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ReversedByEntryId",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ApprovedOnUtc",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "ReversedByEntryId",
                table: "JournalEntries");

            migrationBuilder.DropColumn(
                name: "IsSystemControlled",
                table: "GeneralLedgerAccounts");

            migrationBuilder.DropColumn(
                name: "ParentAccountId",
                table: "GeneralLedgerAccounts");
        }
    }
}
