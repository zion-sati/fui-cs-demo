using System.Runtime.InteropServices.JavaScript;

namespace FuiCsDemo.Shared;

/// <summary>
/// The demo shell's services and events, from <c>Host/shell.mjs</c>. An event's handler runs once straight away with
/// the current value, then on every change until its subscription is disposed.
/// </summary>
public static class DemoShell
{
    /// <summary>The shell's accent as <c>#rrggbb</c>.</summary>
    [JSImport("accentColorHex", "demo.shell")]
    public static extern string AccentColorHex();

    [JSImport("isDarkMode", "demo.shell")]
    public static extern bool IsDarkMode();

    /// <summary>The shell clock's seconds since the site loaded.</summary>
    [JSImport("onClockTickChanged", "demo.shell")]
    public static extern JSSubscription OnClockTickChanged(Action<int> handler);

    [JSImport("onDarkModeChanged", "demo.shell")]
    public static extern JSSubscription OnDarkModeChanged(Action<bool> handler);
}
