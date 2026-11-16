using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseLoanProductsEngine : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AllowMultiTrancheDisbursement",
                table: "LoanProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "AllowTopUp",
                table: "LoanProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "AllowedBranchIdsCsv",
                table: "LoanProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovalStatus",
                table: "LoanProducts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedOnUtc",
                table: "LoanProducts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "CompulsorySavingsPercentage",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "CoolingOffDaysAfterSettlement",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "DeductFeesFromDisbursement",
                table: "LoanProducts",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "EarlySettlementFeePercentage",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveFromDate",
                table: "LoanProducts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EffectiveToDate",
                table: "LoanProducts",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "FeeIncomeAccountId",
                table: "LoanProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "FixedProcessingFee",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "GracePeriodInstallments",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InsuranceFeePercentage",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "InterestCalculationMethod",
                table: "LoanProducts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "InterestIncomeAccountId",
                table: "LoanProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "InterestSlabsJson",
                table: "LoanProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LendingType",
                table: "LoanProducts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<Guid>(
                name: "LoanPortfolioAssetAccountId",
                table: "LoanProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "MaxConcurrentLoans",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxInstallments",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxLoanCycle",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MaxTranchesCount",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinCollateralCoveragePercentage",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "MinCreditScoreRequired",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinInstallments",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "MinLoanCycle",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "MinPrincipalRepaidPercentageForTopUp",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "PenaltyGraceDays",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "PenaltyIncomeAccountId",
                table: "LoanProducts",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyInterestRatePerAnnum",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ProcessingFeePercentage",
                table: "LoanProducts",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "RejectionReason",
                table: "LoanProducts",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RepaymentFrequency",
                table: "LoanProducts",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "RequiredGuarantorsCount",
                table: "LoanProducts",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<Guid>(
                name: "WrittenOffExpenseAccountId",
                table: "LoanProducts",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowMultiTrancheDisbursement",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "AllowTopUp",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "AllowedBranchIdsCsv",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "ApprovalStatus",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "ApprovedOnUtc",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "CompulsorySavingsPercentage",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "CoolingOffDaysAfterSettlement",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "DeductFeesFromDisbursement",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "EarlySettlementFeePercentage",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "EffectiveFromDate",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "EffectiveToDate",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "FeeIncomeAccountId",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "FixedProcessingFee",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "GracePeriodInstallments",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "InsuranceFeePercentage",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "InterestCalculationMethod",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "InterestIncomeAccountId",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "InterestSlabsJson",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "LendingType",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "LoanPortfolioAssetAccountId",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MaxConcurrentLoans",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MaxInstallments",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MaxLoanCycle",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MaxTranchesCount",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MinCollateralCoveragePercentage",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MinCreditScoreRequired",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MinInstallments",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MinLoanCycle",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "MinPrincipalRepaidPercentageForTopUp",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "PenaltyGraceDays",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "PenaltyIncomeAccountId",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "PenaltyInterestRatePerAnnum",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "ProcessingFeePercentage",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "RejectionReason",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "RepaymentFrequency",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "RequiredGuarantorsCount",
                table: "LoanProducts");

            migrationBuilder.DropColumn(
                name: "WrittenOffExpenseAccountId",
                table: "LoanProducts");
        }
    }
}
