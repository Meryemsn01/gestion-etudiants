using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GestionEtudiants.Migrations
{
    /// <inheritdoc />
    public partial class AddMoreFieldsToEtudiants : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "DateNaissance",
                table: "Etudiants",
                type: "datetime(6)",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Email",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Photo",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "Telephone",
                table: "Etudiants",
                type: "longtext",
                nullable: false)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DateNaissance",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "Email",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "Photo",
                table: "Etudiants");

            migrationBuilder.DropColumn(
                name: "Telephone",
                table: "Etudiants");
        }
    }
}
