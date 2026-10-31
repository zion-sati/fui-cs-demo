using FuiCs;

namespace FuiCsDemo.Shared;

public static class ColorUtils
{
    /// <summary>Moves each channel toward white by <paramref name="amount"/> (0 to 1), keeping the alpha.</summary>
    public static Color Lighten(Color color, float amount) => Color.FromRgba(
        Channel(color.R + ((255.0f - color.R) * amount)),
        Channel(color.G + ((255.0f - color.G) * amount)),
        Channel(color.B + ((255.0f - color.B) * amount)),
        color.A);

    /// <summary>Alternating list row fills, from the accent, and the border in the theme's color.</summary>
    public static void ApplyRowBackground(FlexBox row, int index, Theme theme)
    {
        ArgumentNullException.ThrowIfNull(row);
        ArgumentNullException.ThrowIfNull(theme);
        var accent = theme.Colors.Accent;
        row.Background = Color.FromRgba(accent.R, accent.G, accent.B, (index & 1) == 0 ? (byte)0x24 : (byte)0x16);
        row.Border = new Border(1.0f, theme.Colors.Border);
    }

    private static byte Channel(float value) => (byte)Math.Clamp(value, 0.0f, 255.0f);
}
