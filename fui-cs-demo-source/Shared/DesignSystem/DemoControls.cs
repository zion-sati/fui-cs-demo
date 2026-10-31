using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>The sizes the demo's labeled controls, dropdowns and sliders share.</summary>
public static class ControlSizing
{
    private const float ControlFontSize = 14.0f;

    public static LabeledControlSizing Checkbox() => new() { IndicatorSize = 18.0f, LabelFontSize = ControlFontSize };

    public static LabeledControlSizing Radio() => new() { IndicatorSize = 21.0f, LabelFontSize = ControlFontSize };

    public static DropdownSizing Dropdown() => new()
    {
        FieldFontSize = ControlFontSize,
        OptionFontSize = ControlFontSize,
        FieldHeight = 34.0f,
        OptionHeight = 34.0f,
        ChevronBoxSize = 16.0f,
        ChevronIconSize = 12.0f,
    };

    public static SliderSizing Slider() => new() { ThumbSize = 16.0f, TrackThickness = 4.0f };
}

public class DemoCheckbox : Checkbox
{
    public DemoCheckbox(string label)
        : base(label)
    {
        Sizing = ControlSizing.Checkbox();
    }
}

public class DemoRadioButton : RadioButton
{
    public DemoRadioButton(string value, string? label = null)
        : base(value, label)
    {
        Sizing = ControlSizing.Radio();
    }
}

public class DemoRadioGroup : RadioGroup;

public class DemoSlider : Slider
{
    public DemoSlider(float value = 0.0f)
        : base(value)
    {
        Sizing = ControlSizing.Slider();
    }
}

/// <summary>A 240 px dropdown that shows at most five rows before its list scrolls.</summary>
public class DemoDropdown : Dropdown
{
    public DemoDropdown()
    {
        Sizing = ControlSizing.Dropdown();
        Width = Length.Pixels(240.0f);
        MaxVisibleItems = 5;
    }
}

/// <summary>A 40 px tall text input that fills its row, on a field slightly lighter than the surface.</summary>
public class DemoTextInput : TextInput
{
    public DemoTextInput()
    {
        FontSize = 14.0f;
        Height = Length.Pixels(40.0f);
        FillWidth = true;
        Track(Theme.Bind(theme => Colors = new TextInputColors { Background = ColorUtils.Lighten(theme.Colors.Surface, 0.05f) }));
    }
}

/// <summary>A 220 px tall text area that fills its row, on a field slightly lighter than the surface.</summary>
public class DemoTextArea : TextArea
{
    public DemoTextArea(string text = "")
        : base(text)
    {
        FillWidth = true;
        Height = Length.Pixels(220.0f);
        Track(Theme.Bind(theme => Colors = new TextInputColors { Background = ColorUtils.Lighten(theme.Colors.Surface, 0.05f) }));
    }
}

/// <summary>A dialog with Apply and Cancel buttons on a card with 16 px corners.</summary>
public class DemoDialog : Dialog
{
    public DemoDialog(string title, string body)
        : base(title, body)
    {
        AcceptButton.Label = "Apply";
        CancelButton.Label = "Cancel";
        CardCornerRadius = new CornerRadius(16.0f);
    }
}
