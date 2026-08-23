using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsulatTermine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MehrsprachigkeitServiceUndBuchung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "DescriptionArabic",
                table: "Services",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEnglish",
                table: "Services",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameArabic",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEnglish",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Language",
                table: "Appointments",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                defaultValue: "de-DE");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DescriptionArabic",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "DescriptionEnglish",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "NameArabic",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "NameEnglish",
                table: "Services");

            migrationBuilder.DropColumn(
                name: "Language",
                table: "Appointments");
        }
    }
}
