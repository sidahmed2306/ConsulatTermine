using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Security.Claims;
using ConsulatTermine.Application.Configuration;
using ConsulatTermine.Application.Interfaces;
using ConsulatTermine.UI.Authentication;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Options;

namespace ConsulatTermine.UI.Pages.Account;

/// <summary>
/// Anmeldung des Mitarbeiterbereichs.
/// Bewusst als Razor Page und nicht als interaktive Blazor-Komponente umgesetzt:
/// Ein Authentifizierungs-Cookie kann nur im HTTP-Kontext gesetzt werden, nicht aus
/// einem laufenden Blazor-Circuit heraus. Als Formular-Post bekommt die Seite ausserdem
/// den Antiforgery-Schutz von ASP.NET Core ohne Zusatzaufwand.
/// </summary>
[AllowAnonymous]
public sealed class LoginModel : PageModel
{
    private readonly IEmployeeAuthService _authService;
    private readonly EmployeeLoginOptions _loginOptions;
    private readonly ILogger<LoginModel> _logger;

    public LoginModel(
        IEmployeeAuthService authService,
        IOptions<EmployeeLoginOptions> loginOptions,
        ILogger<LoginModel> logger)
    {
        _authService = authService;
        _loginOptions = loginOptions.Value;
        _logger = logger;
    }

    [BindProperty]
    public InputModel Input { get; set; } = new();

    public sealed class InputModel
    {
        [Required(ErrorMessage = "Die Kennung ist erforderlich.")]
        [Display(Name = "Mitarbeiter-Kennung")]
        public string EmployeeCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "Das Passwort ist erforderlich.")]
        [DataType(DataType.Password)]
        [Display(Name = "Passwort")]
        public string Password { get; set; } = string.Empty;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Eine bestehende Anmeldung wird verworfen, damit die Anmeldemaske immer
        // einen definierten Ausgangszustand hat.
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Page();
    }

    public async Task<IActionResult> OnPostAsync(CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        var result = await _authService.LoginAsync(
            Input.EmployeeCode,
            Input.Password,
            cancellationToken);

        if (!result.Success || result.EmployeeId is null || result.Role is null)
        {
            // Die Meldung stammt aus der Anwendungsschicht und ist bewusst unspezifisch.
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Anmeldung fehlgeschlagen.");
            return Page();
        }

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, result.EmployeeId.Value.ToString(CultureInfo.InvariantCulture)),
            new(ClaimTypes.Name, result.EmployeeCode ?? string.Empty),
            new(ClaimTypes.Role, result.Role.Value.ToString())
        };

        if (result.MustChangePassword)
        {
            claims.Add(new Claim(EmployeeClaimTypes.MustChangePassword, "true"));
        }

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(identity),
            new AuthenticationProperties
            {
                IsPersistent = false,
                ExpiresUtc = DateTimeOffset.UtcNow + _loginOptions.SessionTimeout
            });

        _logger.LogInformation("Anmeldung von Mitarbeiter {EmployeeId} erfolgreich.", result.EmployeeId);

        return result.MustChangePassword
            ? Redirect("/employee/change-password")
            : Redirect("/employee/home");
    }
}
