using System.Globalization;
using Blazored.SessionStorage;
using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.Application.Security;
using ConsulatTermine.Domain.Enums;
using ConsulatTermine.Infrastructure;
using ConsulatTermine.Infrastructure.SignalR;
using ConsulatTermine.UI.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using MudBlazor.Services;

var builder = WebApplication.CreateBuilder(args);

// =====================================================================
// KONFIGURATION
// Fehlende kritische Werte fuehren beim Start zu einem klaren Fehler,
// nicht zu spaetem undefiniertem Verhalten. Siehe harness/design.md Abschnitt 9.
// =====================================================================
builder.Services.AddOptions<ApplicationOptions>()
    .Bind(builder.Configuration.GetSection(ApplicationOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<EmailOptions>()
    .Bind(builder.Configuration.GetSection(EmailOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<EmployeeLoginOptions>()
    .Bind(builder.Configuration.GetSection(EmployeeLoginOptions.SectionName))
    .ValidateDataAnnotations()
    .ValidateOnStart();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrWhiteSpace(connectionString))
{
    throw new InvalidOperationException(
        "ConnectionStrings:DefaultConnection ist nicht gesetzt. Lokal wird der Wert ueber "
        + "dotnet user-secrets hinterlegt, in Staging und Production ueber den Secret Store "
        + "der Umgebung. Siehe ConsulatTermine.UI/appsettings.Example.json.");
}

// =====================================================================
// INFRASTRUKTUR
// =====================================================================
builder.Services.AddInfrastructure(connectionString);

// =====================================================================
// AUTHENTIFIZIERUNG UND AUTORISIERUNG
// Die Identitaet liegt in einem serverseitig signierten Cookie. Der Browser-
// Speicher wird dafuer nicht verwendet: er ist vom Benutzer frei veraenderbar
// und damit keine Sicherheitsgrenze. Siehe harness/security.md Abschnitt 1.
// =====================================================================
builder.Services
    .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "ConsulatTermine.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Strict;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

        options.LoginPath = "/employee/login";
        options.LogoutPath = "/employee/logout";
        options.AccessDeniedPath = "/employee/kein-zugriff";

        // Gleitendes Ablaufdatum: aktive Benutzer bleiben angemeldet, inaktive
        // werden nach Ablauf der Leerlaufzeit abgemeldet.
        options.SlidingExpiration = true;
        options.ExpireTimeSpan = builder.Configuration
            .GetSection(EmployeeLoginOptions.SectionName)
            .GetValue<TimeSpan?>(nameof(EmployeeLoginOptions.SessionTimeout))
            ?? TimeSpan.FromMinutes(120);
    });

builder.Services.AddAuthorizationBuilder()
    .AddPolicy(AuthorizationPolicies.MitarbeiterZugriff, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(
                  nameof(EmployeeRole.Employee),
                  nameof(EmployeeRole.ServiceChef),
                  nameof(EmployeeRole.Admin)))
    .AddPolicy(AuthorizationPolicies.DienstplanVerwalten, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(
                  nameof(EmployeeRole.ServiceChef),
                  nameof(EmployeeRole.Admin)))
    .AddPolicy(AuthorizationPolicies.AdministrationVerwalten, policy =>
        policy.RequireAuthenticatedUser()
              .RequireRole(nameof(EmployeeRole.Admin)));

builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<IEmployeeAuthorization, ClaimsEmployeeAuthorization>();

// =====================================================================
// BLAZOR UND UI
// =====================================================================
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSignalR();
builder.Services.AddMudServices();

// Traegt den mehrstufigen Buchungs-Wizard ueber Seitenwechsel hinweg.
// Ausdruecklich kein Sicherheitsmechanismus.
builder.Services.AddBlazoredSessionStorage();

builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");

var app = builder.Build();

// =====================================================================
// LOKALISIERUNG
// =====================================================================
var supportedCultures = new[]
{
    new CultureInfo("de-DE"),
    new CultureInfo("en-US"),
    new CultureInfo("ar-DZ")
};

app.UseRequestLocalization(new RequestLocalizationOptions
{
    DefaultRequestCulture = new RequestCulture("de-DE"),
    SupportedCultures = supportedCultures,
    SupportedUICultures = supportedCultures
});

// =====================================================================
// PIPELINE
// =====================================================================
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapRazorPages();
app.MapBlazorHub();

app.MapHub<DisplayHub>("/hubs/display");
app.MapHub<EmployeeHub>("/hubs/employee");

app.MapFallbackToPage("/_Host");

// =====================================================================
// ERSTSTART
// Ohne Administrator waere die Anwendung nicht verwaltbar. Die Adresse stammt
// aus der Konfiguration und ist nicht im Code hinterlegt.
// =====================================================================
await EnsureInitialAdminAsync(app);

await app.RunAsync();

static async Task EnsureInitialAdminAsync(WebApplication app)
{
    var adminEmail = app.Configuration["Application:InitialAdminEmail"];
    if (string.IsNullOrWhiteSpace(adminEmail))
    {
        UiLog.InitialAdminEmailMissing(app.Logger);
        return;
    }

    await using var scope = app.Services.CreateAsyncScope();
    var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
    await employeeService.EnsureInitialAdminAsync(adminEmail);
}
