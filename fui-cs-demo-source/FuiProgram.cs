using FuiCs;
using FuiCsDemo.Home;

namespace FuiCsDemo;

/// <summary>
/// The home route: a 100,000-item virtual list beside the basic controls showcase (common controls, images, a nested
/// scroll box and control foundations), with the time since the page started from the shell's clock.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() =>
        Application.CreateManaged(static () => new HomePage());
}
