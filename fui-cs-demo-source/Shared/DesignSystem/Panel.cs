using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>
/// A rounded surface that stacks its children: bordered and shadowed in the theme's colors, with less padding and
/// margin in the compact layout, so nested panels leave more of a phone's width to their content.
/// </summary>
public class Panel : FlexBox
{
    public Panel()
    {
        FlexDirection = FlexDirection.Column;
        AlignItems = AlignItems.Start;
        CornerRadius = new CornerRadius(16.0f);
        Track(Responsive.Subscribe(ApplySpacing));
        Track(Theme.Bind(ApplyDemoTheme));
    }

    private void ApplySpacing(bool compact)
    {
        Padding = new Thickness(compact ? 14.0f : 24.0f);
        Margin = new Thickness(compact ? 6.0f : 12.0f);
    }

    private void ApplyDemoTheme(Theme theme)
    {
        Border = new Border(1.0f, theme.Colors.Border);
        Background = theme.Colors.Surface;
        DropShadow = new DropShadow(theme.Colors.PanelShadow, 0.0f, 2.0f, 7.0f);
    }
}
