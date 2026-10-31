using FuiCs;

namespace FuiCsDemo.TextFonts;

/// <summary>
/// The text and fonts route: a sign-in form password managers recognize, a text area reconfigured live, rich text
/// spans, and app-loaded DejaVu fonts with a color emoji fallback.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.CreateManaged(static () => new TextFontsPage());
}
