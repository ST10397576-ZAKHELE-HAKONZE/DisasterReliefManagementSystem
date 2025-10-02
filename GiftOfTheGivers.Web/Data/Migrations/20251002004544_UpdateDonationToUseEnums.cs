using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateDonationToUseEnums : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // FIX: Step 1. Data cleanup is required BEFORE altering column types.
            // Delete all Donation rows where the old 'Type' or 'Status' values are not
            // standard (e.g., not 'Cash', 'Goods', 'Received', 'Allocated').
            // If you have standard data like 'Cash' or 'Goods', you would use
            // SQL to map 'Cash' to 1 and 'Goods' to 2, but since the error shows
            // 'clothes', we must delete the incompatible data.
            migrationBuilder.Sql(@"
                DELETE FROM Donations 
                WHERE [Type] IS NOT NULL AND [Type] NOT IN ('Cash', 'Goods')
                OR [Status] IS NOT NULL AND [Status] NOT IN ('Received', 'In Transit', 'Allocated')
            ");

            // --- Type Column Alteration ---
            // Drop existing default constraint on Type column if it exists
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Donations");

            // Add the new Type column as integer
            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Donations",
                type: "int",
                nullable: false,
                defaultValue: 1); // Set a default value (e.g., 1 for Financial)

            // --- Status Column Alteration ---
            // Drop existing default constraint on Status column if it exists
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donations");

            // Add the new Status column as integer
            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Donations",
                type: "int",
                nullable: false,
                defaultValue: 1); // Set a default value (e.g., 1 for Received)
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse operations for Type (back to nvarchar(max))
            migrationBuilder.DropColumn(
                name: "Type",
                table: "Donations");

            migrationBuilder.AddColumn<string>(
                name: "Type",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            // Reverse operations for Status (back to nvarchar(max))
            migrationBuilder.DropColumn(
                name: "Status",
                table: "Donations");

            migrationBuilder.AddColumn<string>(
                name: "Status",
                table: "Donations",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
