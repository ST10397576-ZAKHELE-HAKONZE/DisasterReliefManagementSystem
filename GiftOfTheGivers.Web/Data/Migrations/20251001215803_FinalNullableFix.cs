using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalNullableFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // This command changes the existing EndDate column to allow NULL values.
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "ReliefProjects",
                type: "datetime2",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "datetime2");

            // We do NOT need AlterColumn for StartDate because it's already NOT NULL
            // and the type (DateTime) is correct.
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // This command reverts EndDate back to NOT NULL (if needed to rollback).
            migrationBuilder.AlterColumn<DateTime>(
                name: "EndDate",
                table: "ReliefProjects",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "datetime2",
                oldNullable: true);
        }

    }
}
