using System.Runtime.InteropServices.JavaScript;

namespace FuiCsDemo.Shared;

/// <summary>The demo shell's clock, from <c>Host/shell.mjs</c>.</summary>
public static class DemoShell
{
    [JSImport("nowUnixSeconds", "demo.shell")]
    public static extern int NowUnixSeconds();

    /// <summary>
    /// Calls <paramref name="handler"/> with the time in Unix seconds straight away, then once a second until the
    /// subscription is disposed.
    /// </summary>
    [JSImport("onClockTick", "demo.shell")]
    public static extern JSSubscription OnClockTick(Action<int> handler);
}
