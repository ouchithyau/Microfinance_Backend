using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddEnterpriseCentersAndGroupsLogic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Centers_Branches_BranchId",
                table: "Centers");

            migrationBuilder.DropForeignKey(
                name: "FK_Centers_Users_FieldOfficerId",
                table: "Centers");

            migrationBuilder.DropForeignKey(
                name: "FK_SolidarityGroups_Centers_CenterId",
                table: "SolidarityGroups");

            migrationBuilder.DropIndex(
                name: "IX_SolidarityGroups_CenterId",
                table: "SolidarityGroups");

            migrationBuilder.DropIndex(
                name: "IX_Centers_BranchId",
                table: "Centers");

            migrationBuilder.DropIndex(
                name: "IX_Centers_FieldOfficerId",
                table: "Centers");

            migrationBuilder.AddColumn<int>(
                name: "CompletedCgtSessions",
                table: "SolidarityGroups",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "GroupSecuritySavings",
                table: "SolidarityGroups",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<DateTime>(
                name: "GrtApprovedOnUtc",
                table: "SolidarityGroups",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "SolidarityGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsGrtApproved",
                table: "SolidarityGroups",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<Guid>(
                name: "FieldOfficerId",
                table: "Centers",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AddColumn<Guid>(
                name: "CenterLeaderId",
                table: "Centers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CenterSecretaryId",
                table: "Centers",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "Latitude",
                table: "Centers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<string>(
                name: "LocationAddress",
                table: "Centers",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "Longitude",
                table: "Centers",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<int>(
                name: "MaxGroupsAllowed",
                table: "Centers",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsGeoVerified",
                table: "CenterMeetingSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "CenterMeetingSchedules",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<double>(
                name: "RecordedLatitude",
                table: "CenterMeetingSchedules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "RecordedLongitude",
                table: "CenterMeetingSchedules",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<decimal>(
                name: "TotalSavingsCollected",
                table: "CenterMeetingSchedules",
                type: "numeric(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.CreateTable(
                name: "CenterMeetingAttendances",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CenterMeetingId = table.Column<Guid>(type: "uuid", nullable: false),
                    CustomerId = table.Column<Guid>(type: "uuid", nullable: false),
                    IsPresent = table.Column<bool>(type: "boolean", nullable: false),
                    SavingsCollected = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    Remarks = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CenterMeetingAttendances", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CenterMeetingAttendances");

            migrationBuilder.DropColumn(
                name: "CompletedCgtSessions",
                table: "SolidarityGroups");

            migrationBuilder.DropColumn(
                name: "GroupSecuritySavings",
                table: "SolidarityGroups");

            migrationBuilder.DropColumn(
                name: "GrtApprovedOnUtc",
                table: "SolidarityGroups");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SolidarityGroups");

            migrationBuilder.DropColumn(
                name: "IsGrtApproved",
                table: "SolidarityGroups");

            migrationBuilder.DropColumn(
                name: "CenterLeaderId",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "CenterSecretaryId",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "Latitude",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "LocationAddress",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "Longitude",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "MaxGroupsAllowed",
                table: "Centers");

            migrationBuilder.DropColumn(
                name: "IsGeoVerified",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "RecordedLatitude",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "RecordedLongitude",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "TotalSavingsCollected",
                table: "CenterMeetingSchedules");

            migrationBuilder.AlterColumn<Guid>(
                name: "FieldOfficerId",
                table: "Centers",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_SolidarityGroups_CenterId",
                table: "SolidarityGroups",
                column: "CenterId");

            migrationBuilder.CreateIndex(
                name: "IX_Centers_BranchId",
                table: "Centers",
                column: "BranchId");

            migrationBuilder.CreateIndex(
                name: "IX_Centers_FieldOfficerId",
                table: "Centers",
                column: "FieldOfficerId");

            migrationBuilder.AddForeignKey(
                name: "FK_Centers_Branches_BranchId",
                table: "Centers",
                column: "BranchId",
                principalTable: "Branches",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Centers_Users_FieldOfficerId",
                table: "Centers",
                column: "FieldOfficerId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_SolidarityGroups_Centers_CenterId",
                table: "SolidarityGroups",
                column: "CenterId",
                principalTable: "Centers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
