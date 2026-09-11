using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CityChoir.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRegistrationNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "RegistrationNumber",
                table: "Users",
                type: "varchar(255)",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "RegistrationSequences",
                columns: table => new
                {
                    Year = table.Column<int>(type: "int", nullable: false),
                    Part = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NextNumber = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrationSequences", x => new { x.Year, x.Part });
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("""
                UPDATE Users u
                INNER JOIN (
                    SELECT Id,
                           CONCAT(
                               Part,
                               '/',
                               YEAR(CreatedAt),
                               '/',
                               LPAD(
                                   ROW_NUMBER() OVER (
                                       PARTITION BY Part, YEAR(CreatedAt)
                                       ORDER BY CreatedAt, Id
                                   ),
                                   3,
                                   '0'
                               )
                           ) AS GeneratedRegistrationNumber
                    FROM Users
                ) numbered ON numbered.Id = u.Id
                SET u.RegistrationNumber = numbered.GeneratedRegistrationNumber
                """);

            migrationBuilder.Sql("""
                INSERT INTO RegistrationSequences (Year, Part, NextNumber)
                SELECT YEAR(CreatedAt), Part,
                       MAX(CAST(SUBSTRING_INDEX(RegistrationNumber, '/', -1) AS UNSIGNED))
                FROM Users
                GROUP BY YEAR(CreatedAt), Part
                """);

            migrationBuilder.Sql("""
                ALTER TABLE Users
                MODIFY RegistrationNumber varchar(255) NOT NULL
                """);

            migrationBuilder.CreateIndex(
                name: "IX_Users_RegistrationNumber",
                table: "Users",
                column: "RegistrationNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RegistrationSequences");

            migrationBuilder.DropIndex(
                name: "IX_Users_RegistrationNumber",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "RegistrationNumber",
                table: "Users");
        }
    }
}
