using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable
#pragma warning disable CS8981

namespace ConsulatTermine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class current : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CurrentEmployeeId",
                table: "Appointments",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_CurrentEmployeeId",
                table: "Appointments",
                column: "CurrentEmployeeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Appointments_Employees_CurrentEmployeeId",
                table: "Appointments",
                column: "CurrentEmployeeId",
                principalTable: "Employees",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Appointments_Employees_CurrentEmployeeId",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_CurrentEmployeeId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "CurrentEmployeeId",
                table: "Appointments");
        }
    }
}
