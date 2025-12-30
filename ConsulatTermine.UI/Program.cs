
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








var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddBlazoredSessionStorage();
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();

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











builder.Services.AddMudServices();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var supportedCultures = new[] { "de", "en", "ar" };

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.SetDefaultCulture("ar");
    options.AddSupportedCultures(supportedCultures);
    options.AddSupportedUICultures(supportedCultures);
});



var app = builder.Build();


app.UseRequestLocalization();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.MapBlazorHub();

app.MapHub<DisplayHub>("/hubs/display");
app.MapHub<EmployeeHub>("/hubs/employee");



app.MapFallbackToPage("/_Host");



app.Run();
