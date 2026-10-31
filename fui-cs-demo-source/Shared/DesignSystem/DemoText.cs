using FuiCs;

namespace FuiCsDemo.Shared;

public enum DemoTextStyle
{
    Heading1,
    Heading2,
    Heading3,
    Lead,
    Label,
    Body,
    BodySecondary,
    Caption,
}

/// <summary>Text in one of the demo's styles: its size and weight, and its color, which follows the theme.</summary>
public class DemoText : Text
{
    private DemoTextStyle style;

    public DemoText(string content = "", DemoTextStyle style = DemoTextStyle.Body)
        : base(content)
    {
        this.style = style;
        ApplyTypography();
        Track(Theme.Bind(ApplyDemoTheme));
    }

    public DemoTextStyle Style
    {
        get => style;
        set
        {
            style = value;
            ApplyTypography();
            ApplyDemoTheme(Theme.Active.Value);
        }
    }

    private void ApplyTypography()
    {
        (FontSize, FontWeight) = style switch
        {
            DemoTextStyle.Heading1 => (26.0f, FontWeight.Bold),
            DemoTextStyle.Heading2 => (22.0f, FontWeight.Bold),
            DemoTextStyle.Heading3 => (18.0f, FontWeight.Bold),
            DemoTextStyle.Lead => (17.0f, FontWeight.Regular),
            DemoTextStyle.Label => (16.0f, FontWeight.Regular),
            DemoTextStyle.BodySecondary => (14.0f, FontWeight.Regular),
            DemoTextStyle.Caption => (12.0f, FontWeight.Regular),
            _ => (15.0f, FontWeight.Regular),
        };
    }

    private void ApplyDemoTheme(Theme theme) =>
        TextColor = style is DemoTextStyle.BodySecondary or DemoTextStyle.Caption ? theme.Colors.TextMuted : theme.Colors.TextPrimary;
}
