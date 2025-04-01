using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEtudiants.Migrations
{
    /// <inheritdoc />
    public partial class AddCoefficientToMatiere : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Coefficient",
                table: "Matieres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Etudiants",
                type: "longtext",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Coefficient",
                table: "Matieres");

            migrationBuilder.UpdateData(
                table: "Etudiants",
                keyColumn: "Photo",
                keyValue: null,
                column: "Photo",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "Photo",
                table: "Etudiants",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "longtext",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");
        }
    }
}
