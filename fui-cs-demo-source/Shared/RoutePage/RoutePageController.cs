using System.Globalization;

using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>Something a route page owns besides its view: it follows the theme and is disposed with the page.</summary>
public interface IRoutePageLifecycleOwner : IDisposable
{
    void ApplyTheme(Theme theme);
}

/// <summary>
/// Runs a route page: the demo theme from the shell's dark mode and accent, the route, viewport and action count in the
/// view, and the action button. The page is the application's managed controller.
/// </summary>
public sealed class RoutePageController : ManagedApplicationController
{
    private readonly IRoutePageLifecycleOwner? lifecycleOwner;
    private readonly List<IDisposable> subscriptions = [];
    private bool isDarkMode;
    private readonly Color accentColor;

    public RoutePageController(
        RoutePageModel model,
        IReadOnlyList<RoutePageSection> sections,
        IRoutePageLifecycleOwner? lifecycleOwner = null)
    {
        ArgumentNullException.ThrowIfNull(model);
        Model = model;
        this.lifecycleOwner = lifecycleOwner;
        isDarkMode = DemoShell.IsDarkMode();
        accentColor = ParseAccentColor(DemoShell.AccentColorHex(), model.AccentColor);
        View = new RoutePageView(model, sections);
        View.SyncViewportLayout();
        View.ActionButton.Click += _ => ++model.ActionCount.Value;
        subscriptions.Add(DemoShell.OnDarkModeChanged(SetDarkMode));
        SyncTheme();
        subscriptions.Add(Theme.Bind(ApplyTheme));
        subscriptions.Add(Navigation.CurrentRoute.Subscribe(View.SetCurrentRoute));
        subscriptions.Add(Application.ViewportWidth.Subscribe(_ => View.SyncViewportLayout()));
        subscriptions.Add(model.ActionCount.Subscribe(View.SetActionCount));
        View.SetCurrentRoute(Navigation.CurrentRoute.Value);
        View.SetActionCount(model.ActionCount.Value);
    }

    public RoutePageModel Model { get; }

    public RoutePageView View { get; }

    public int ActionCount => Model.ActionCount.Value;

    public override Node GetRoot() => View.Root;

    public override void Mount()
    {
        Application.SetCaption("EffinDOM FUI-CS Demo • " + Model.Title);
        base.Mount();
    }

    public override void Dispose()
    {
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
        lifecycleOwner?.Dispose();
        View.Dispose();
        base.Dispose();
    }

    /// <summary>The shell's <c>#rrggbb</c> accent, or <paramref name="fallback"/> when it is not one.</summary>
    public static Color ParseAccentColor(string? value, Color fallback)
    {
        if (value is not { Length: 7 } || value[0] != '#'
            || !uint.TryParse(value.AsSpan(1), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var rgb))
        {
            return fallback;
        }
        return Color.FromRgb((byte)(rgb >> 16), (byte)(rgb >> 8), (byte)rgb);
    }

    private void ApplyTheme(Theme theme)
    {
        View.ApplyTheme(theme);
        lifecycleOwner?.ApplyTheme(theme);
    }

    private void SetDarkMode(bool isDark)
    {
        if (isDarkMode == isDark)
        {
            return;
        }
        isDarkMode = isDark;
        SyncTheme();
    }


    private void SyncTheme() => Theme.UseCustom(DemoTheme.Generate(isDarkMode, accentColor));
}
