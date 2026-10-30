using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>The demo's route paths, as FuiCsDemo.csproj lists them, from the site's root.</summary>
public static class DemoRoutes
{
    public const string Home = "/";

    public const string ImmediateDrawing = "/immediate-drawing/";

    public const string AdvancedControls = "/advanced-controls/";

    public const string TemplatedControls = "/templated-controls/";

    /// <summary>The demo's source code.</summary>
    public const string Source = "https://github.com/zion-sati/fui-cs-demo";

    /// <summary>
    /// A font the demo's routes share, from the site's <c>fonts/</c> folder: the runtime's fonts, and DejaVu Sans,
    /// which the demo ships in <c>assets/fonts/</c> with its license.
    /// </summary>
    public static string SharedFontUrl(string assetFile)
    {
        ArgumentNullException.ThrowIfNull(assetFile);
        return "/fonts/" + assetFile;
    }
}
