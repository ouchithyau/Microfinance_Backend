using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseAuthAndRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Centers_CenterId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SolidarityGroups_SolidarityGroupId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanAccounts_Customers_CustomerId",
                table: "LoanAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanAccounts_LoanProducts_LoanProductId",
                table: "LoanAccounts");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "ForcePasswordChangeOnNextLogin",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordChangedOnUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Users",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiryUtc",
                table: "Users",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "LoanAccounts",
                type: "character varying(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "EntryNumber",
                table: "JournalEntries",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "NicNumber",
                table: "Customers",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Customers",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "character varying(30)",
                maxLength: 30,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: false),
                    ExpiresOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    CreatedByIp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RefreshTokens_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_LoanAccounts_AccountNumber",
                table: "LoanAccounts",
                column: "AccountNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries",
                column: "EntryNumber",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers",
                column: "CustomerCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Customers_NicNumber",
                table: "Customers",
                column: "NicNumber");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Centers_CenterId",
                table: "Customers",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SolidarityGroups_SolidarityGroupId",
                table: "Customers",
                column: "SolidarityGroupId",
                principalTable: "SolidarityGroups",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanAccounts_Customers_CustomerId",
                table: "LoanAccounts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanAccounts_LoanProducts_LoanProductId",
                table: "LoanAccounts",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_Centers_CenterId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_Customers_SolidarityGroups_SolidarityGroupId",
                table: "Customers");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanAccounts_Customers_CustomerId",
                table: "LoanAccounts");

            migrationBuilder.DropForeignKey(
                name: "FK_LoanAccounts_LoanProducts_LoanProductId",
                table: "LoanAccounts");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropIndex(
                name: "IX_LoanAccounts_AccountNumber",
                table: "LoanAccounts");

            migrationBuilder.DropIndex(
                name: "IX_JournalEntries_EntryNumber",
                table: "JournalEntries");

            migrationBuilder.DropIndex(
                name: "IX_Customers_CustomerCode",
                table: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Customers_NicNumber",
                table: "Customers");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "ForcePasswordChangeOnNextLogin",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEndUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordChangedOnUtc",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiryUtc",
                table: "Users");

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "LoanAccounts",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(40)",
                oldMaxLength: 40);

            migrationBuilder.AlterColumn<string>(
                name: "EntryNumber",
                table: "JournalEntries",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "NicNumber",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "CustomerCode",
                table: "Customers",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(30)",
                oldMaxLength: 30);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Branches_BranchId",
                table: "Customers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_Centers_CenterId",
                table: "Customers",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Customers_SolidarityGroups_SolidarityGroupId",
                table: "Customers",
                column: "SolidarityGroupId",
                principalTable: "SolidarityGroups",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_LoanAccounts_Customers_CustomerId",
                table: "LoanAccounts",
                column: "CustomerId",
                principalTable: "Customers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_LoanAccounts_LoanProducts_LoanProductId",
                table: "LoanAccounts",
                column: "LoanProductId",
                principalTable: "LoanProducts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
