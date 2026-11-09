using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Microfinance.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UpdateAuditFilters : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "JournalEntryLines",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "JournalEntryLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "JournalEntryLines",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "JournalEntryLines",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "JournalEntryLines",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "CenterMeetingSchedules",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "CenterMeetingSchedules",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "CenterMeetingSchedules",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastModifiedAt",
                table: "CenterMeetingSchedules",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "LastModifiedBy",
                table: "CenterMeetingSchedules",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "JournalEntryLines");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "LastModifiedAt",
                table: "CenterMeetingSchedules");

            migrationBuilder.DropColumn(
                name: "LastModifiedBy",
                table: "CenterMeetingSchedules");
        }
    }
}
