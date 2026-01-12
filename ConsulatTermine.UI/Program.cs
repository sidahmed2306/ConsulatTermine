using Microsoft.EntityFrameworkCore;
using ConsulatTermine.Infrastructure.Persistence;
using Infrastructure.SignalR;
using MudBlazor.Services;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Infrastructure.Services;
using Blazored.SessionStorage;
using ConsulatTermine.Application.Interfaces.Booking;
using ConsulatTermine.Infrastructure.Services.Booking;
using ConsulatTermine.UI.Authentication;
using ConsulatTermine.Infrastructure.SignalR;
using System.Globalization;
using Microsoft.AspNetCore.Localization;

var builder = WebApplication.CreateBuilder(args);

// =====================
// DATABASE
// =====================
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// =====================
// BASIC SERVICES
// =====================
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

// =====================
// LOCALIZATION (WICHTIG)
// =====================
builder.Services.AddLocalization(options =>
{
    options.ResourcesPath = "Resources";
});

// =====================
// APPLICATION SERVICES
// =====================
builder.Services.AddScoped<IServiceDayOverrideService, ServiceDayOverrideService>();
builder.Services.AddScoped<IServiceService, ServiceService>();
builder.Services.AddScoped<IEmployeeService, EmployeeService>();
builder.Services.AddScoped<IAppointmentService, AppointmentService>();
builder.Services.AddScoped<IEmployeeAssignmentService, EmployeeAssignmentService>();
builder.Services.AddScoped<IWorkingHoursService, WorkingHoursService>();
builder.Services.AddScoped<IWorkingScheduleService, WorkingScheduleService>();
builder.Services.AddScoped<IWorkingScheduleOverviewService, WorkingScheduleOverviewService>();
builder.Services.AddScoped<IBookingReferenceGenerator, BookingReferenceGenerator>();
builder.Services.AddScoped<IBookingValidationService, BookingValidationService>();
builder.Services.AddScoped<ISlotAvailabilityService, SlotAvailabilityService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IWorkingSchedulePlanService, WorkingSchedulePlanService>();
builder.Services.AddScoped<IAppointmentCalendarQueryService, AppointmentCalendarQueryService>();
builder.Services.AddScoped<IEmailService, SmtpEmailService>();
builder.Services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
builder.Services.AddScoped<EmployeeSessionService>();
builder.Services.AddSingleton<IWaitingRoomNotifier, WaitingRoomNotifier>();

// =====================
// UI
// =====================
builder.Services.AddMudServices();

var app = builder.Build();

// =====================
// REQUEST LOCALIZATION
// =====================
var supportedCultures = new[]
{
    new CultureInfo("de-DE"),
    new CultureInfo("en-US"),
    new CultureInfo("ar-DZ")
};

var localizationOptions = new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("de-DE"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
};

app.UseRequestLocalization(localizationOptions);

// =====================
// MIDDLEWARE PIPELINE
// =====================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.MapBlazorHub();

app.MapHub<DisplayHub>("/hubs/display");
app.MapHub<EmployeeHub>("/hubs/employee");

app.MapFallbackToPage("/_Host");

app.Run();
