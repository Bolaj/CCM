using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityChoir.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailTokenType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TokenType",
                table: "EmailVerificationTokens",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TokenType",
                table: "EmailVerificationTokens");
        }
    }
}
