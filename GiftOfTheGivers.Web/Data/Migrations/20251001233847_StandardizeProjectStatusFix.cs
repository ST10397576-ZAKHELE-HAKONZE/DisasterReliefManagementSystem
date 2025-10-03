using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class StandardizeProjectStatusFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. ADD a temporary integer column to hold the new Status values
            migrationBuilder.AddColumn<int>(
                name: "StatusNew",
                table: "ReliefProjects",
                type: "int",
                nullable: true); // Must be nullable during conversion

            // 2. COPY and CONVERT data from the old string column to the new integer column
            // We use SQL to map the old string values to the correct enum integer values:
            // Planning = 1, Active = 2, OnHold = 3, Completed = 4
            // Defaulting any unrecognized value to Planning (1).
            migrationBuilder.Sql(@"
                UPDATE ReliefProjects
                SET StatusNew = 
                    CASE Status
                        WHEN 'Planning' THEN 1
                        WHEN 'Active' THEN 2
                        WHEN 'On Hold' THEN 3
                        WHEN 'Completed' THEN 4
                        ELSE 1 -- Default to Planning if old data is bad
                    END;
            ");

            // 3. DROP the old 'Status' column (which held the string values)
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReliefProjects");

            // 4. RENAME the temporary 'StatusNew' column to 'Status'
            migrationBuilder.RenameColumn(
                name: "StatusNew",
                table: "ReliefProjects",
                newName: "Status");

            // 5. ALTER the column to be NOT NULL (since the enum is required)
            migrationBuilder.AlterColumn<int>(
                name: "Status",
                table: "ReliefProjects",
                type: "int",
                nullable: false,
                defaultValue: 1, // Set default value for new rows
                oldNullable: true); // Original temp column was nullable
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // To undo the Up migration, we must reverse the steps:
            // 1. Add back the temporary string column
            migrationBuilder.AddColumn<string>(
                name: "StatusNew",
                table: "ReliefProjects",
                type: "nvarchar(max)",
                nullable: true);

            // 2. Copy data back (converting integer enum values to strings)
            migrationBuilder.Sql(@"
                UPDATE ReliefProjects
                SET StatusNew = 
                    CASE Status
                        WHEN 1 THEN 'Planning'
                        WHEN 2 THEN 'Active'
                        WHEN 3 THEN 'On Hold'
                        WHEN 4 THEN 'Completed'
                        ELSE 'Planning'
                    END;
            ");

            // 3. Drop the new integer column
            migrationBuilder.DropColumn(
                name: "Status",
                table: "ReliefProjects");

            // 4. Rename the temporary string column back to Status
            migrationBuilder.RenameColumn(
                name: "StatusNew",
                table: "ReliefProjects",
                newName: "Status");

            // 5. Alter the column to be NOT NULL
            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "ReliefProjects",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "Planning",
                oldNullable: true);
        }
    }
}