using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseCustomersAndKycLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Guarantors",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "ExistingDebtObligations",
                table: "Customers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsPep",
                table: "Customers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "KycRejectionReason",
                table: "Customers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "KycVerifiedOnUtc",
                table: "Customers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyExpenses",
                table: "Customers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "MonthlyIncome",
                table: "Customers",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "NomineeName",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomineeNic",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomineePhone",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "NomineeRelationship",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "RiskRating",
                table: "Customers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "FullName",
                table: "BlacklistRecords",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "LiftReason",
                table: "BlacklistRecords",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "LiftedOnUtc",
                table: "BlacklistRecords",
                type: "timestamp with time zone",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Guarantors");

            migrationBuilder.DropColumn(
                name: "ExistingDebtObligations",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "IsPep",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycRejectionReason",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "KycVerifiedOnUtc",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MonthlyExpenses",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "MonthlyIncome",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineeName",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineeNic",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineePhone",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "NomineeRelationship",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "RiskRating",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FullName",
                table: "BlacklistRecords");

            migrationBuilder.DropColumn(
                name: "LiftReason",
                table: "BlacklistRecords");

            migrationBuilder.DropColumn(
                name: "LiftedOnUtc",
                table: "BlacklistRecords");
        }
    }
}
