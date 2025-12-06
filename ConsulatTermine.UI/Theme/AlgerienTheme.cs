using MudBlazor;

namespace ConsulatTermine.UI.Theme
{
    public static class AlgeriaTheme
    {
        public static MudTheme DefaultTheme = new MudTheme()
        {
            PaletteLight = new PaletteLight
            {
                Primary = "#006400",           // Dunkelgrün (Hauptfarbe)
                Secondary = "#008000",         // Grün (sekundär / Akzent)
                Tertiary = "#004d26",          // Optional: dunkleres Grün als tertiär

                Info = "#17a2b8",              // Info — türkis-blau (z.B. Hinweise)
                Success = "#28a745",           // Erfolg → typisch Grün
                Warning = "#ffc107",           // Warnung → Gelb/Orange
                Error = "#c8102e",             // Fehler / Rot (Flaggen-Rot)

                Background = "#FFFFFF",        // Weiß – Hintergrund
                BackgroundGray = "#f5f5f5",    // Hellgrau für Alternativflächen
                Surface = "#FFFFFF",           // Weiß – für Cards etc.

                AppbarBackground = "#006400",  // Dunkelgrün – AppBar
                AppbarText = "#FFFFFF",
                DrawerBackground = "#FFFFFF",
                DrawerText = "#000000",
                DrawerIcon = "#000000",

                TextPrimary = "#000000",
                TextSecondary = "#444444",
                TextDisabled = "rgba(0,0,0, 0.38)",

                Divider = "rgba(0,0,0, 0.12)",
                LinesDefault = "rgba(0,0,0, 0.12)",

                // falls du weitere UI-Elemente hast
                TableLines = "rgba(0,0,0, 0.12)"
            },
            // Keine PaletteDark — bleibt null
            LayoutProperties = new LayoutProperties()
            {
                DefaultBorderRadius = "4px",
                DrawerWidthLeft = "260px",
                DrawerWidthRight = "260px",
                AppbarHeight = "64px"
            }
        };
    }
}
