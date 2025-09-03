using MudBlazor;

namespace Client.Theme;

public static class TimeStockTheme
{
    public static readonly MudTheme Base = new()
    {
        // Palette claire
        PaletteLight = new PaletteLight
        {
            Primary    = Colors.Blue.Default,
            Secondary  = Colors.DeepPurple.Default,
            Success    = Colors.Green.Accent4,
            Error      = Colors.Red.Default,
            Background = Colors.Gray.Lighten5,
            Surface    = Colors.Gray.Lighten4
        },

        // Palette sombre
        PaletteDark = new PaletteDark
        {
            Primary       = Colors.Blue.Lighten2,
            Secondary     = Colors.DeepPurple.Lighten2,
            Success       = Colors.Green.Accent3,
            Error         = Colors.Red.Lighten1,
            Background    = "#1E1E2E",
            Surface       = "#27293D",
            TextPrimary   = Colors.Gray.Lighten3,
            TextSecondary = Colors.Gray.Lighten2
        },

        LayoutProperties = new LayoutProperties { DefaultBorderRadius = "6px" },

        // Instanciation avec DefaultTypography (et non BaseTypography)
        Typography = new Typography
        {
            Default = new DefaultTypography
            {
                FontFamily = new[] { "Inter", "sans-serif" }
            }
        }
    };
}
