using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityChoir.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRehearsalVenue : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Venue",
                table: "Rehearsals",
                type: "longtext",
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Venue",
                table: "Rehearsals");
        }
    }
}
