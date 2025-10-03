using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixIncidentReportIDType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // CRITICAL FIX: Ensure ReportID (the Primary Key) is an integer type.
            // This fixes the 'Unable to cast System.String to System.Int32' error.
            migrationBuilder.AlterColumn<int>(
                name: "ReportID",
                table: "IncidentReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            // FIX 1: Changing Status from string to int (for the IncidentStatus enum)
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "IncidentReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            // FIX 2: Changing Severity from string to int (for the IncidentSeverity enum)
            migrationBuilder.AlterColumn<int>(
                name: "Severity",
                table: "IncidentReports",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Revert Severity
            migrationBuilder.AlterColumn<string>(
                name: "Severity",
                table: "IncidentReports",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Revert Status
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "IncidentReports",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            // Revert ReportID
            migrationBuilder.AlterColumn<string>(
                name: "ReportID",
                table: "IncidentReports",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");
        }
    }
}
