window.culture = {
  set: function (culture) {
    // .NET Standard Cookie Name für RequestLocalization
    // Format: c=xx-XX|uic=xx-XX
    const value = `c=${culture}|uic=${culture}`;

    // 1 Jahr gültig
    const maxAge = 60 * 60 * 24 * 365;

    document.cookie = `.AspNetCore.Culture=${encodeURIComponent(
      value
    )}; path=/; max-age=${maxAge}; samesite=lax`;

    // Voll-Reload, damit Blazor Server neue Culture sauber übernimmt
    window.location.reload();
  },
};
