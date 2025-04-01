using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEtudiants.Migrations
{
    /// <inheritdoc />
    public partial class AddCINandCodeApogeeToEtudiants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CIN",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "CodeApogee",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CIN",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "CodeApogee",
                table: "Etudiants");
        }
    }
}
