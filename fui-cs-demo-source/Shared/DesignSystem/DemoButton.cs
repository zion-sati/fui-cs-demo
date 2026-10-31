using FuiCs;

namespace FuiCsDemo.Shared;

public enum DemoButtonTone
{
    Primary,
    Secondary,
}

/// <summary>A rounded button in the accent (primary) or on the surface (secondary), recolored with the theme.</summary>
public class DemoButton : Button
{
    private readonly DemoButtonTone tone;

    public DemoButton(string label, DemoButtonTone tone = DemoButtonTone.Secondary)
        : base(label)
    {
        this.tone = tone;
        CornerRadius = new CornerRadius(12.0f);
        Padding = new Thickness(18.0f, 10.0f);
        FontSize = 15.0f;
        Track(Theme.Bind(ApplyDemoTheme));
    }

    private void ApplyDemoTheme(Theme theme)
    {
        Background = tone == DemoButtonTone.Primary ? theme.Colors.Accent : theme.Colors.Surface;
        TextColor = tone == DemoButtonTone.Primary ? theme.Colors.TextOnAccent : theme.Colors.TextPrimary;
    }
}

/// <summary>A rounded button in the theme's own button colors.</summary>
public class PrimaryButton : Button
{
    public PrimaryButton(string label)
        : base(label)
    {
        CornerRadius = new CornerRadius(12.0f);
        Padding = new Thickness(18.0f, 10.0f);
        FontSize = 14.0f;
    }
}
