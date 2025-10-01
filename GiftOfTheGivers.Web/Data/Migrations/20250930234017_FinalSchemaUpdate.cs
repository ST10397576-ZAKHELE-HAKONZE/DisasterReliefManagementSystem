using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GiftOfTheGivers.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class FinalSchemaUpdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // 1. CREATE the missing Donors table
            migrationBuilder.CreateTable(
                name: "Donors",
                columns: table => new
                {
                    DonorID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    ContactNumber = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donors", x => x.DonorID);
                });


            // 2. CREATE the missing Donations table
            migrationBuilder.CreateTable(
                name: "Donations",
                columns: table => new
                {
                    DonationID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Amount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DateReceived = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DonorID = table.Column<int>(type: "int", nullable: false),
                    RecordedByUserId = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    ReliefProjectProjectID = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Donations", x => x.DonationID);
                    // This Foreign Key relies on AspNetUsers (exists)
                    table.ForeignKey(
                        name: "FK_Donations_AspNetUsers_RecordedByUserId",
                        column: x => x.RecordedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    // This Foreign Key relies on Donors (created above)
                    table.ForeignKey(
                        name: "FK_Donations_Donors_DonorID",
                        column: x => x.DonorID,
                        principalTable: "Donors",
                        principalColumn: "DonorID",
                        onDelete: ReferentialAction.Restrict);
                    // This Foreign Key relies on ReliefProjects (exists)
                    table.ForeignKey(
                        name: "FK_Donations_ReliefProjects_ReliefProjectProjectID",
                        column: x => x.ReliefProjectProjectID,
                        principalTable: "ReliefProjects",
                        principalColumn: "ProjectID");
                });

            // 3. CREATE Indices ONLY for the table(s) created in this file (Donations)

            migrationBuilder.CreateIndex(
                name: "IX_Donations_DonorID",
                table: "Donations",
                column: "DonorID");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_RecordedByUserId",
                table: "Donations",
                column: "RecordedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Donations_ReliefProjectProjectID",
                table: "Donations",
                column: "ReliefProjectProjectID");

            // All other CreateTable and CreateIndex calls must be DELETED!
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Drop only the tables created in this Up method
            migrationBuilder.DropTable(
                name: "Donations");


            migrationBuilder.DropTable(
                name: "Donors");

            // All other DropTable calls must be DELETED!
        }
    }
}