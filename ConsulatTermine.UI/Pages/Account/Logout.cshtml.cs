using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ConsulatTermine.UI.Pages.Account;

/// <summary>
/// Abmeldung. Loescht das Authentifizierungs-Cookie im HTTP-Kontext; aus einem
/// Blazor-Circuit heraus ist das nicht moeglich.
/// </summary>
[AllowAnonymous]
public sealed class LogoutModel : PageModel
{
    public async Task<IActionResult> OnGetAsync()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Redirect("/employee/login");
    }
}
