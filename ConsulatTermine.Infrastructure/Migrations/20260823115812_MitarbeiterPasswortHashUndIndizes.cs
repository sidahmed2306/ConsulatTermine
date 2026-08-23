using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsulatTermine.Infrastructure.Migrations
{
    /// <summary>
    /// Stellt die Mitarbeitertabelle auf gehashte Passwoerter um und ergaenzt Indizes.
    ///
    /// ACHTUNG, vor dem Einspielen auf einer bestehenden Datenbank pruefen:
    ///
    /// 1. TemporaryPassword und PasswordResetToken werden ersatzlos entfernt. Die dort
    ///    gespeicherten Klartext-Passwoerter lassen sich nicht in Hashes ueberfuehren und
    ///    gelten ohnehin als kompromittiert. Nach der Migration hat jeder Mitarbeiter
    ///    PasswordHash = NULL und kann sich nicht anmelden, bis er ueber
    ///    "Passwort vergessen" ein neues Passwort gesetzt hat. Die Mitarbeiter sind vorab
    ///    zu informieren.
    ///
    /// 2. Die neuen eindeutigen Indizes auf Employees.EmployeeCode und Employees.Email
    ///    schlagen fehl, wenn der Bestand Duplikate enthaelt. Vorher pruefen:
    ///
    ///        SELECT EmployeeCode, COUNT(*) FROM Employees
    ///        GROUP BY EmployeeCode HAVING COUNT(*) > 1;
    ///        SELECT Email, COUNT(*) FROM Employees
    ///        GROUP BY Email HAVING COUNT(*) > 1;
    ///
    /// 3. Die Laengenbegrenzungen kuerzen keine Daten, sondern schlagen fehl, wenn ein
    ///    vorhandener Wert laenger ist als die neue Obergrenze.
    ///
    /// Down() stellt Struktur und Spalten wieder her, aber keine Passwoerter.
    /// </summary>
    public partial class MitarbeiterPasswortHashUndIndizes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ServiceDayOverrides_ServiceDayOverrides_ServiceDayOverrideId",
                table: "ServiceDayOverrides");

            migrationBuilder.DropIndex(
                name: "IX_ServiceDayOverrides_ServiceDayOverrideId",
                table: "ServiceDayOverrides");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "ServiceDayOverrideId",
                table: "ServiceDayOverrides");

            migrationBuilder.DropColumn(
                name: "IdentityUserId",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PasswordResetToken",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "TemporaryPassword",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Floor",
                table: "Services",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Holidays",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "FailedLoginAttempts",
                table: "Employees",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "LockoutEndsAt",
                table: "Employees",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordHash",
                table: "Employees",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetTokenHash",
                table: "Employees",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Appointments",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Appointments",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Appointments",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "CancelToken",
                table: "Appointments",
                type: "nvarchar(64)",
                maxLength: 64,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookingReference",
                table: "Appointments",
                type: "nvarchar(40)",
                maxLength: 40,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Holidays_Date",
                table: "Holidays",
                column: "Date");

            migrationBuilder.CreateIndex(
                name: "IX_Employees_Email",
                table: "Employees",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees",
                column: "EmployeeCode",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Employees_PasswordResetTokenHash",
                table: "Employees",
                column: "PasswordResetTokenHash");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_BookingReference",
                table: "Appointments",
                column: "BookingReference");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId_Date",
                table: "Appointments",
                columns: new[] { "ServiceId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_Status_IsVisibleInWaitingRoom",
                table: "Appointments",
                columns: new[] { "Status", "IsVisibleInWaitingRoom" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Holidays_Date",
                table: "Holidays");

            migrationBuilder.DropIndex(
                name: "IX_Employees_Email",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_EmployeeCode",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Employees_PasswordResetTokenHash",
                table: "Employees");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_BookingReference",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_ServiceId_Date",
                table: "Appointments");

            migrationBuilder.DropIndex(
                name: "IX_Appointments_Status_IsVisibleInWaitingRoom",
                table: "Appointments");

            migrationBuilder.DropColumn(
                name: "FailedLoginAttempts",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "LockoutEndsAt",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PasswordHash",
                table: "Employees");

            migrationBuilder.DropColumn(
                name: "PasswordResetTokenHash",
                table: "Employees");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Floor",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Services",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AddColumn<int>(
                name: "ServiceDayOverrideId",
                table: "ServiceDayOverrides",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Holidays",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "LastName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "FirstName",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "EmployeeCode",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(20)",
                oldMaxLength: 20);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AddColumn<string>(
                name: "IdentityUserId",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PasswordResetToken",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TemporaryPassword",
                table: "Employees",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "PhoneNumber",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "FullName",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "Email",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(256)",
                oldMaxLength: 256);

            migrationBuilder.AlterColumn<string>(
                name: "CancelToken",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(64)",
                oldMaxLength: 64,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "BookingReference",
                table: "Appointments",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(40)",
                oldMaxLength: 40);

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDayOverrides_ServiceDayOverrideId",
                table: "ServiceDayOverrides",
                column: "ServiceDayOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.AddForeignKey(
                name: "FK_ServiceDayOverrides_ServiceDayOverrides_ServiceDayOverrideId",
                table: "ServiceDayOverrides",
                column: "ServiceDayOverrideId",
                principalTable: "ServiceDayOverrides",
                principalColumn: "Id");
        }
    }
}
