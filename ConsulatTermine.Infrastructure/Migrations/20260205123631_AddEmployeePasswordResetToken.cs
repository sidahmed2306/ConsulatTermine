using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsulatTermine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddEmployeePasswordResetToken : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "PasswordResetTokenExpiresAt",
                table: "Employees",
                type: "datetime2",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenExpiresAt",
                table: "Employees");
        }
    }
}
