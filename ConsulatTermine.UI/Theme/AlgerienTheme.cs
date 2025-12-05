using MudBlazor;

namespace ConsulatTermine.UI.Theme
{
    public static class AlgeriaTheme
    {
        public static MudTheme Theme = new MudTheme()
        {
            PaletteLight = new PaletteLight()
            {
                // Algerische Flagge: Grün, Rot, Weiß
                Primary = "#006233",      // Grün
                Secondary = "#D52B1E",    // Rot
                Background = "#F5F5F5",   // Hellgrau / fast weiß
                Surface = "#FFFFFF",      // Karten-Hintergrund
                AppbarBackground = "#006233",
                AppbarText = "#FFFFFF",
                TextPrimary = "#222222",
                TextSecondary = "#555555"
            },

            LayoutProperties = new LayoutProperties()
            {
                // Runde Ecken wie in deinen Screenshots
                DefaultBorderRadius = "12px"
            },

            Typography = new Typography()
{
    Body1 = new Body1Typography()
    {
        FontFamily = new[] { "Inter", "Roboto", "Arial", "sans-serif" }
    },
    Button = new ButtonTypography()
    {
        FontWeight = "600",
        TextTransform = "none"
    },
    H5 = new H5Typography()
    {
        FontWeight = "700"
    }
}

        };
    }
}
