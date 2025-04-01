using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEtudiants.Migrations
{
    /// <inheritdoc />
    public partial class AddClasseTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Classe",
                table: "Etudiants");

            migrationBuilder.AddColumn<int>(
                name: "ClasseId",
                table: "Matieres",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "CodeApogee",
                table: "Etudiants",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<string>(
                name: "CIN",
                table: "Etudiants",
                type: "varchar(255)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "longtext")
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<int>(
                name: "ClasseId",
                table: "Etudiants",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Classes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nom = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Classes", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Matieres_ClasseId",
                table: "Matieres",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_CIN",
                table: "Etudiants",
                column: "CIN",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_ClasseId",
                table: "Etudiants",
                column: "ClasseId");

            migrationBuilder.CreateIndex(
                name: "IX_Etudiants_CodeApogee",
                table: "Etudiants",
                column: "CodeApogee",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Etudiants_Classes_ClasseId",
                table: "Etudiants",
                column: "ClasseId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Matieres_Classes_ClasseId",
                table: "Matieres",
                column: "ClasseId",
                principalTable: "Classes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Etudiants_Classes_ClasseId",
                table: "Etudiants");

            migrationBuilder.DropForeignKey(
                name: "FK_Matieres_Classes_ClasseId",
                table: "Matieres");

            migrationBuilder.DropTable(
                name: "Classes");

            migrationBuilder.DropIndex(
                name: "IX_Matieres_ClasseId",
                table: "Matieres");

            migrationBuilder.DropIndex(
                name: "IX_Etudiants_CIN",
                table: "Etudiants");

            migrationBuilder.DropIndex(
                name: "IX_Etudiants_ClasseId",
                table: "Etudiants");

            migrationBuilder.DropIndex(
                name: "IX_Etudiants_CodeApogee",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "ClasseId",
                table: "Matieres");

            migrationBuilder.DropColumn(
                name: "ClasseId",
                table: "Etudiants");

            migrationBuilder.UpdateData(
                table: "Etudiants",
                keyColumn: "CodeApogee",
                keyValue: null,
                column: "CodeApogee",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CodeApogee",
                table: "Etudiants",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Etudiants",
                keyColumn: "CIN",
                keyValue: null,
                column: "CIN",
                value: "");

            migrationBuilder.AlterColumn<string>(
                name: "CIN",
                table: "Etudiants",
                type: "longtext",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(255)",
                oldNullable: true)
                .Annotation("MySql:CharSet", "utf8mb4")
                .OldAnnotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Classe",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }
    }
}
