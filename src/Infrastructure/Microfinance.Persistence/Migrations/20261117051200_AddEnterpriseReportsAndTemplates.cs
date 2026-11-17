using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseReportsAndTemplates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepaymentTransactions_Users_CollectedByUserId",
                table: "RepaymentTransactions");

            migrationBuilder.AlterColumn<Guid>(
                name: "CollectedByUserId",
                table: "RepaymentTransactions",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<decimal>(
                name: "AdvanceAllocated",
                table: "RepaymentTransactions",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "BankName",
                table: "RepaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ClearanceStatus",
                table: "RepaymentTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsReversed",
                table: "RepaymentTransactions",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "PaymentMode",
                table: "RepaymentTransactions",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "ReferenceNumber",
                table: "RepaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ReversalReason",
                table: "RepaymentTransactions",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "ReversedOnUtc",
                table: "RepaymentTransactions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "DueDate",
                table: "RepaymentSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<int>(
                name: "InstallmentNumber",
                table: "RepaymentSchedules",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "InterestDue",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<bool>(
                name: "IsPaid",
                table: "RepaymentSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "PaidDate",
                table: "RepaymentSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidInterest",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidPenalty",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PaidPrincipal",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PenaltyAmount",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "PrincipalDue",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "WaivedPenalty",
                table: "RepaymentSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "SavedReportTemplates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    TemplateName = table.Column<string>(type: "text", nullable: false),
                    ReportType = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    FilterParametersJson = table.Column<string>(type: "text", nullable: false),
                    ScheduleFrequency = table.Column<string>(type: "text", nullable: false),
                    RecipientEmailsCsv = table.Column<string>(type: "text", nullable: true),
                    IsActive = table.Column<bool>(type: "boolean", nullable: false),
                    LastGeneratedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: true),
                    LastModifiedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    LastModifiedBy = table.Column<string>(type: "text", nullable: true),
                    IsDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SavedReportTemplates", x => x.Id);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_RepaymentTransactions_Users_CollectedByUserId",
                table: "RepaymentTransactions",
                column: "CollectedByUserId",
                principalTable: "Users",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_RepaymentTransactions_Users_CollectedByUserId",
                table: "RepaymentTransactions");

            migrationBuilder.DropTable(
                name: "SavedReportTemplates");

            migrationBuilder.DropColumn(
                name: "AdvanceAllocated",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "BankName",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ClearanceStatus",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "IsReversed",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "PaymentMode",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ReferenceNumber",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ReversalReason",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "ReversedOnUtc",
                table: "RepaymentTransactions");

            migrationBuilder.DropColumn(
                name: "DueDate",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "InstallmentNumber",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "InterestDue",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "IsPaid",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PaidDate",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PaidInterest",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PaidPenalty",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PaidPrincipal",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PenaltyAmount",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "PrincipalDue",
                table: "RepaymentSchedules");

            migrationBuilder.DropColumn(
                name: "WaivedPenalty",
                table: "RepaymentSchedules");

            migrationBuilder.AlterColumn<Guid>(
                name: "CollectedByUserId",
                table: "RepaymentTransactions",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_RepaymentTransactions_Users_CollectedByUserId",
                table: "RepaymentTransactions",
                column: "CollectedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
