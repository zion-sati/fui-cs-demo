using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>
/// The compact layout, for a viewport narrower than <see cref="CompactMaxWidth"/>: one column, no minimum page width,
/// tighter padding.
/// </summary>
public static class Responsive
{
    public const float CompactMaxWidth = 860.0f;

    public static bool IsCompact
    {
        get
        {
            var width = Application.ViewportWidth.Value;
            return width > 0.0f && width < CompactMaxWidth;
        }
    }

    /// <summary>
    /// Calls <paramref name="apply"/> with whether the compact layout applies, now and whenever the viewport's width
    /// changes, until the subscription is disposed: a node tracks it, a page's code ends it with the page
    /// (<see cref="Application.OnUnmount"/>).
    /// </summary>
    public static IDisposable Subscribe(Action<bool> apply)
    {
        ArgumentNullException.ThrowIfNull(apply);
        apply(IsCompact);
        return Application.ViewportWidth.Subscribe(_ => apply(IsCompact));
    }
}
