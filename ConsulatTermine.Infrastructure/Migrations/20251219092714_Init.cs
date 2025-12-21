using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ConsulatTermine.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdentityUserId = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Holidays",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Holidays", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Services",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CapacityPerSlot = table.Column<int>(type: "int", nullable: true),
                    SlotDurationMinutes = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Services", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Appointments",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CheckedInAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    BookingReference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PersonIndex = table.Column<int>(type: "int", nullable: false),
                    IsMainPerson = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Appointments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Appointments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "EmployeeServiceAssignments",
                columns: table => new
                {
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ServiceId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmployeeServiceAssignments", x => new { x.EmployeeId, x.ServiceId });
                    table.ForeignKey(
                        name: "FK_EmployeeServiceAssignments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmployeeServiceAssignments_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingSchedulePlans",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    ValidFromDate = table.Column<DateOnly>(type: "date", nullable: false),
                    ValidToDate = table.Column<DateOnly>(type: "date", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingSchedulePlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingSchedulePlans_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ServiceDayOverrides",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    Date = table.Column<DateTime>(type: "datetime2", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: true),
                    IsClosed = table.Column<bool>(type: "bit", nullable: false),
                    CapacityPerSlotOverride = table.Column<int>(type: "int", nullable: true),
                    IsWeeklyOverride = table.Column<bool>(type: "bit", nullable: false),
                    WeeklyDay = table.Column<int>(type: "int", nullable: true),
                    WorkingSchedulePlanId = table.Column<int>(type: "int", nullable: false),
                    ServiceDayOverrideId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ServiceDayOverrides", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ServiceDayOverrides_ServiceDayOverrides_ServiceDayOverrideId",
                        column: x => x.ServiceDayOverrideId,
                        principalTable: "ServiceDayOverrides",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ServiceDayOverrides_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ServiceDayOverrides_WorkingSchedulePlans_WorkingSchedulePlanId",
                        column: x => x.WorkingSchedulePlanId,
                        principalTable: "WorkingSchedulePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "WorkingHours",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ServiceId = table.Column<int>(type: "int", nullable: false),
                    WorkingSchedulePlanId = table.Column<int>(type: "int", nullable: false),
                    Day = table.Column<int>(type: "int", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkingHours", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkingHours_Services_ServiceId",
                        column: x => x.ServiceId,
                        principalTable: "Services",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkingHours_WorkingSchedulePlans_WorkingSchedulePlanId",
                        column: x => x.WorkingSchedulePlanId,
                        principalTable: "WorkingSchedulePlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Appointments_ServiceId",
                table: "Appointments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_EmployeeServiceAssignments_ServiceId",
                table: "EmployeeServiceAssignments",
                column: "ServiceId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDayOverrides_ServiceDayOverrideId",
                table: "ServiceDayOverrides",
                column: "ServiceDayOverrideId");

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDayOverrides_ServiceId_WorkingSchedulePlanId_Date",
                table: "ServiceDayOverrides",
                columns: new[] { "ServiceId", "WorkingSchedulePlanId", "Date" });

            migrationBuilder.CreateIndex(
                name: "IX_ServiceDayOverrides_WorkingSchedulePlanId",
                table: "ServiceDayOverrides",
                column: "WorkingSchedulePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_ServiceId_WorkingSchedulePlanId_Day",
                table: "WorkingHours",
                columns: new[] { "ServiceId", "WorkingSchedulePlanId", "Day" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingHours_WorkingSchedulePlanId",
                table: "WorkingHours",
                column: "WorkingSchedulePlanId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkingSchedulePlans_ServiceId_IsActive",
                table: "WorkingSchedulePlans",
                columns: new[] { "ServiceId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_WorkingSchedulePlans_ServiceId_ValidFromDate_ValidToDate",
                table: "WorkingSchedulePlans",
                columns: new[] { "ServiceId", "ValidFromDate", "ValidToDate" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Appointments");

            migrationBuilder.DropTable(
                name: "EmployeeServiceAssignments");

            migrationBuilder.DropTable(
                name: "Holidays");

            migrationBuilder.DropTable(
                name: "ServiceDayOverrides");

            migrationBuilder.DropTable(
                name: "WorkingHours");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "WorkingSchedulePlans");

            migrationBuilder.DropTable(
                name: "Services");
        }
    }
}
