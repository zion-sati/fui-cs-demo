using FuiCs;

namespace FuiCsDemo.Advanced;

/// <summary>
/// The advanced route: transitions and smooth scrolling, a worker driving a progress bar, real fetch requests, and
/// drag-and-drop reordering in an inner scroll box.
/// </summary>
internal static class FuiProgram
{
    public static IApplication CreateApplication() => Application.CreateManaged(static () => new AdvancedPage());
}
