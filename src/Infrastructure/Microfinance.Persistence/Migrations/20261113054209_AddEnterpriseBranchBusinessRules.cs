using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseBranchBusinessRules : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CurrentVaultBalance",
                table: "Branches",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsOpenForTransactions",
                table: "Branches",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastClosedOnUtc",
                table: "Branches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastOpenedOnUtc",
                table: "Branches",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxCashVaultLimit",
                table: "Branches",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MaxLoanApprovalLimit",
                table: "Branches",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<double>(
                name: "ServiceRadiusKm",
                table: "Branches",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CurrentVaultBalance",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "IsOpenForTransactions",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "LastClosedOnUtc",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "LastOpenedOnUtc",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "MaxCashVaultLimit",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "MaxLoanApprovalLimit",
                table: "Branches");

            migrationBuilder.DropColumn(
                name: "ServiceRadiusKm",
                table: "Branches");
        }
    }
}
