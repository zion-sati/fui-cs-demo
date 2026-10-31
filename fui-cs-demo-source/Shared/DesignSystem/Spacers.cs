using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>The demo's vertical rhythm, and spacers for it.</summary>
public static class Spacing
{
    public const float PageSectionGap = 24.0f;
    public const float PanelSectionGap = 16.0f;
    public const float HeadingToBodyGap = 12.0f;
    public const float HeadingToBodyTightGap = 8.0f;
    public const float TitleToSupportingGap = 10.0f;
    public const float LabelToControlGap = 6.0f;
    public const float MicroStackGap = 4.0f;

    public static FlexBox Vertical(float height) => new FlexBox().Height(height);

    public static FlexBox Horizontal(float width) => new FlexBox().Width(width);
}
