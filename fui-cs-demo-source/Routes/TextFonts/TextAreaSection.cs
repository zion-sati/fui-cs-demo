using System;
using System.Globalization;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.TextFonts;

/// <summary>
/// A text area with toggles, radio groups and a dropdown that reconfigure it live, and status lines for its focus,
/// selection and settings. The scroll bar toggles and policy radios mirror each other.
/// </summary>
internal sealed class TextAreaSection
{
    private const float FixedLineHeight = 28.0f;

    private readonly DemoTextArea textArea = new DemoTextArea("Line one\nLine two\nLine three\nLonger content so scrollbar policy is easy to spot.")
        .Placeholder("Type notes here or paste sample content. Use the controls below to reconfigure the TextArea live.")
        .NodeId("demo-advanced:text-area");
    private readonly DemoCheckbox readOnlyToggle = new("Read-only");
    private readonly DemoCheckbox wrappingToggle = new DemoCheckbox("Wrapping").IsChecked(true);
    private readonly DemoCheckbox alwaysVerticalToggle = new("Always show vertical scrollbar");
    private readonly DemoCheckbox neverVerticalToggle = new("Hide vertical scrollbar");
    private readonly DemoCheckbox alwaysHorizontalToggle = new("Always show horizontal scrollbar");
    private readonly DemoCheckbox neverHorizontalToggle = new("Hide horizontal scrollbar");
    private readonly DemoRadioGroup verticalPolicyGroup = PolicyGroup("Vertical");
    private readonly DemoRadioGroup horizontalPolicyGroup = PolicyGroup("Horizontal");
    private readonly DemoRadioGroup lineHeightGroup = new DemoRadioGroup()
        .Radio(new DemoRadioButton("normal", "Line height: Normal"), new DemoRadioButton("fixed-28", "Line height: Fixed 28 px"))
        .SelectedIndex(0);
    private readonly DemoRadioGroup fontModeGroup = new DemoRadioGroup()
        .Radio(new DemoRadioButton("variable", "Text font: Variable width"), new DemoRadioButton("mono", "Text font: Monospace"))
        .SelectedIndex(0);
    private readonly DemoDropdown visibilityDropdown = new DemoDropdown()
        .Items(
            new DropdownItem("normal", "Visibility: Normal - keep layout reserved and content rendered"),
            new DropdownItem("hidden", "Visibility: Hidden - keep layout reserved but stop painting content"),
            new DropdownItem("collapsed", "Visibility: Collapsed - remove layout space and hide the content"))
        .SelectedIndex(0)
        .Width(350.0f)
        .MaxWidth(100.0f, Unit.Percent);
    private readonly DemoText focusStatusText = new("", DemoTextStyle.Caption);
    private readonly DemoText selectionStatusText = new("", DemoTextStyle.Caption);
    private readonly DemoText settingsStatusText = new("", DemoTextStyle.BodySecondary);
    private bool syncingVerticalPolicy;
    private bool syncingHorizontalPolicy;

    public TextAreaSection()
    {
        readOnlyToggle.Changed += e =>
        {
            textArea.IsReadOnly = e.IsChecked != false;
            SyncStatus();
        };
        wrappingToggle.Changed += e =>
        {
            var enabled = e.IsChecked != false;
            textArea.Wrapping = enabled;
            if (enabled)
            {
                horizontalPolicyGroup.SelectedIndex = 0;
            }
            SyncHorizontalPolicy();
            SyncStatus();
        };
        alwaysVerticalToggle.Changed += e => OnPolicyToggle(e, neverVerticalToggle, vertical: true);
        neverVerticalToggle.Changed += e => OnPolicyToggle(e, alwaysVerticalToggle, vertical: true);
        alwaysHorizontalToggle.Changed += e => OnPolicyToggle(e, neverHorizontalToggle, vertical: false);
        neverHorizontalToggle.Changed += e => OnPolicyToggle(e, alwaysHorizontalToggle, vertical: false);
        textArea.FocusChanged += _ => SyncStatus();
        textArea.SelectionChanged += _ => SyncStatus();
        textArea.TextChanged += _ => SyncStatus();
        verticalPolicyGroup.Changed += _ => SyncVerticalPolicy();
        horizontalPolicyGroup.Changed += _ => SyncHorizontalPolicy();
        lineHeightGroup.Changed += _ => SyncLineHeight();
        fontModeGroup.Changed += _ => SyncFontMode();
        visibilityDropdown.Changed += _ => SyncVisibility();
        Application.OnUnmount(Theme.Active.Subscribe(_ => SyncFontMode()).Dispose);
        SyncFontMode();
        SyncVerticalPolicy();
        SyncHorizontalPolicy();
        SyncLineHeight();
        SyncVisibility();
        SyncStatus();
    }

    /// <summary>The section's content, under its heading in the page's text inputs panel.</summary>
    public Node[] Build() =>
    [
        new DemoText("Text Area", DemoTextStyle.Heading3),
        Spacing.Vertical(Spacing.TitleToSupportingGap),
        textArea,
        Spacing.Vertical(14.0f),
        new DemoText("Use the quick toggles for common changes, or the radio groups when you want an exact scrollbar or line-height setting.", DemoTextStyle.BodySecondary),
        Spacing.Vertical(14.0f),
        FlexBox.Row(
                FlexBox.Column(
                    readOnlyToggle,
                    Spacing.Vertical(8.0f),
                    wrappingToggle,
                    Spacing.Vertical(8.0f),
                    alwaysVerticalToggle,
                    Spacing.Vertical(8.0f),
                    neverVerticalToggle,
                    Spacing.Vertical(8.0f),
                    alwaysHorizontalToggle,
                    Spacing.Vertical(8.0f),
                    neverHorizontalToggle),
                Spacing.Horizontal(96.0f),
                FlexBox.Column(
                    verticalPolicyGroup,
                    Spacing.Vertical(12.0f),
                    horizontalPolicyGroup,
                    Spacing.Vertical(12.0f),
                    lineHeightGroup,
                    Spacing.Vertical(12.0f),
                    fontModeGroup,
                    Spacing.Vertical(12.0f),
                    visibilityDropdown))
            .FlexWrap(FlexWrap.Wrap),
        Spacing.Vertical(14.0f),
        focusStatusText,
        Spacing.Vertical(6.0f),
        selectionStatusText,
        Spacing.Vertical(6.0f),
        settingsStatusText,
    ];

    private static DemoRadioGroup PolicyGroup(string axis) => new DemoRadioGroup()
        .Radio(
            new DemoRadioButton("auto", axis + " scrollbar: Auto"),
            new DemoRadioButton("always", axis + " scrollbar: Always"),
            new DemoRadioButton("never", axis + " scrollbar: Never"))
        .SelectedIndex(0);

    /// <summary>A scroll bar toggle unchecks its opposite and picks the matching policy radio.</summary>
    private void OnPolicyToggle(CheckboxChangedEventArgs e, DemoCheckbox opposite, bool vertical)
    {
        if (vertical ? syncingVerticalPolicy : syncingHorizontalPolicy)
        {
            return;
        }
        if (e.IsChecked != false)
        {
            opposite.IsChecked = false;
        }
        var (always, never, group) = vertical
            ? (alwaysVerticalToggle, neverVerticalToggle, verticalPolicyGroup)
            : (alwaysHorizontalToggle, neverHorizontalToggle, horizontalPolicyGroup);
        SetSyncing(vertical, true);
        group.SelectedIndex = never.IsChecked == true ? 2 : always.IsChecked == true ? 1 : 0;
        SetSyncing(vertical, false);
        if (vertical)
        {
            SyncVerticalPolicy();
        }
        else
        {
            SyncHorizontalPolicy();
        }
        SyncStatus();
    }

    private void SetSyncing(bool vertical, bool syncing)
    {
        if (vertical)
        {
            syncingVerticalPolicy = syncing;
        }
        else
        {
            syncingHorizontalPolicy = syncing;
        }
    }

    private void SyncVerticalPolicy()
    {
        syncingVerticalPolicy = true;
        var value = verticalPolicyGroup.SelectedValue;
        textArea.VerticalScrollBarVisibility = Visibility(value);
        alwaysVerticalToggle.IsChecked = value == "always";
        neverVerticalToggle.IsChecked = value == "never";
        syncingVerticalPolicy = false;
    }

    private void SyncHorizontalPolicy()
    {
        syncingHorizontalPolicy = true;
        var value = horizontalPolicyGroup.SelectedValue;
        textArea.HorizontalScrollBarVisibility = Visibility(value);
        alwaysHorizontalToggle.IsChecked = value == "always";
        neverHorizontalToggle.IsChecked = value == "never";
        syncingHorizontalPolicy = false;
    }

    private static ScrollBarVisibility Visibility(string? policy) => policy switch
    {
        "always" => ScrollBarVisibility.Visible,
        "never" => ScrollBarVisibility.Hidden,
        _ => ScrollBarVisibility.Auto,
    };

    private void SyncLineHeight()
    {
        if (lineHeightGroup.SelectedValue == "fixed-28")
        {
            textArea.LineHeight = FixedLineHeight;
        }
        else
        {
            textArea.LineHeight = null;
            lineHeightGroup.SelectedIndex = 0;
        }
        SyncStatus();
    }

    private void SyncFontMode()
    {
        var fonts = Theme.Active.Value.Fonts;
        if (fontModeGroup.SelectedValue == "mono")
        {
            textArea.FontFamily = fonts.MonoFamily;
            textArea.FontSize = fonts.SizeMono;
        }
        else
        {
            textArea.FontFamily = fonts.BodyFamily;
            textArea.FontSize = fonts.SizeBody;
            fontModeGroup.SelectedIndex = 0;
        }
        SyncStatus();
    }

    private void SyncVisibility()
    {
        textArea.Visibility = visibilityDropdown.SelectedIndex switch
        {
            1 => FuiCs.Visibility.Hidden,
            2 => FuiCs.Visibility.Collapsed,
            _ => FuiCs.Visibility.Normal,
        };
        SyncStatus();
    }

    private void SyncStatus()
    {
        var length = textArea.Text.Length.ToString(CultureInfo.InvariantCulture);
        focusStatusText.Text = "Focus: " + (textArea.IsFocused ? "focused" : "blurred") + " • Text length: " + length;
        selectionStatusText.Text = "Selection: " + textArea.SelectionStart.ToString(CultureInfo.InvariantCulture) + "-"
            + textArea.SelectionEnd.ToString(CultureInfo.InvariantCulture);
        settingsStatusText.Text = "Read-only: " + (textArea.IsReadOnly ? "on" : "off")
            + " • Wrapping: " + (wrappingToggle.IsChecked == true ? "on" : "off")
            + " • Visibility: " + (visibilityDropdown.SelectedIndex switch { 1 => "hidden", 2 => "collapsed", _ => "normal" })
            + " • Vertical: " + verticalPolicyGroup.SelectedValue
            + " • Horizontal: " + horizontalPolicyGroup.SelectedValue
            + " • Line height: " + (lineHeightGroup.SelectedValue == "fixed-28" ? "fixed 28px" : "normal")
            + " • Font: " + (fontModeGroup.SelectedValue == "mono" ? "monospace" : "variable");
    }
}
