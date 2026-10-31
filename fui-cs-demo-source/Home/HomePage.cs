using System;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Home;

/// <summary>
/// The home route: a 100,000-item virtual list beside the basic controls showcase, which reports the time since the
/// page started, opens a dialog with the current selections, and scrolls through common controls, images, a nested
/// scroll box and control foundations. In the compact layout the list sits above the showcase.
/// </summary>
internal sealed class HomePage : ManagedApplicationController
{
    private const float CompactListPanelHeight = 320.0f;
    // Tall enough for the showcase under the list to keep a usable height of its own; the page scrolls.
    private const float CompactPageMinHeight = 1240.0f;

    private readonly DropdownItem[] dropdownItems =
    [
        new("tiny", "Tiny"),
        new("small", "Small"),
        new("medium", "Medium"),
        new("large", "Large"),
        new("xlarge", "Extra large"),
    ];

    private readonly DemoText elapsedText = new("Time since app started: 00:00:00", DemoTextStyle.Lead);
    private readonly DemoText summaryText = new("Selections: waiting for interaction.", DemoTextStyle.BodySecondary);
    private readonly PrimaryButton actionButton = new("Open dialog form");
    private readonly PrimaryButton foundationsToggleButton = new("Disable scoped child");
    private readonly PrimaryButton foundationsScopedButton = new("Scoped child action");
    private readonly DemoText foundationsStatusText = new("", DemoTextStyle.BodySecondary);
    private readonly DemoText foundationsFocusText = new("", DemoTextStyle.BodySecondary);
    private readonly DemoText foundationsActionText = new("", DemoTextStyle.BodySecondary);
    private readonly DemoText keyTargetText = new DemoText("", DemoTextStyle.Body).Selectable(false);
    private readonly FlexBox keyTargetBox;
    private readonly Panel foundationsScopeBox;
    private readonly DemoDialog dialog = new("Demo dialog form", "No selection captured yet.");
    private readonly DemoDropdown dropdown = new();
    private readonly DemoCheckbox triStateCheckbox = new("Tri-state checkbox");
    private readonly DemoCheckbox biStateCheckbox = new("Bi-state checkbox");
    private readonly DemoRadioGroup qualityRadioGroup = new();
    private readonly DemoSlider horizontalSlider = new(40.0f);
    private readonly DemoSlider verticalSlider = new(65.0f);
    private readonly DemoVirtualList virtualList = new("VirtualList", 100_000, 62.0f);
    private readonly Bitmap bitmap = HomeBitmap.Create();
    private readonly Panel listPanel;
    private readonly FlexBox columnGap = Spacing.Horizontal(24.0f);
    private readonly FlexBox columns;
    private readonly SelectionArea root;
    private readonly IDisposable clock;
    private int? startTick;
    private bool foundationsScopeEnabled = true;
    private bool foundationsScopedFocused;
    private int foundationsActionCount;
    private bool keyTargetFocused;
    private string lastKey = "";

    public HomePage()
    {
        Theme.UseSystem();
        keyTargetBox = new FlexBox()
            .Width(280.0f)
            .Padding(12.0f, 10.0f, 12.0f, 10.0f)
            .CornerRadius(12.0f)
            .Child(keyTargetText)
            .Focusable(true);
        foundationsScopeBox = new Panel()
            .Width(Length.Auto)
            .Child(new DemoText("This child button inherits enabled/disabled from its parent scope.", DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(10.0f))
            .Child(foundationsScopedButton)
            .Child(Spacing.Vertical(8.0f))
            .Child(foundationsFocusText)
            .Child(Spacing.Vertical(8.0f))
            .Child(foundationsActionText);

        dialog.Accepted += () => summaryText.Text = "Dialog accepted.";
        dialog.Cancelled += () => summaryText.Text = "Dialog cancelled.";
        dropdown.Items(dropdownItems).SelectedIndex(2);
        dropdown.Changed += _ => ShowSelectionSummary();
        triStateCheckbox.IsThreeState(true).IsChecked(null);
        triStateCheckbox.Changed += _ => ShowSelectionSummary();
        biStateCheckbox.IsChecked(true);
        biStateCheckbox.Changed += _ => ShowSelectionSummary();
        qualityRadioGroup
            .Radio(new DemoRadioButton("balanced", "Balanced"), new DemoRadioButton("quality", "Quality first"), new DemoRadioButton("speed", "Speed first"))
            .SelectedIndex(0);
        qualityRadioGroup.Changed += _ => ShowSelectionSummary();
        horizontalSlider.Minimum(0.0f).Maximum(100.0f).Step(5.0f).Length(300.0f).Orientation(Orientation.Horizontal);
        horizontalSlider.Changed += _ => ShowSelectionSummary();
        verticalSlider.Minimum(0.0f).Maximum(100.0f).Step(5.0f).Length(150.0f).Orientation(Orientation.Vertical);
        verticalSlider.Changed += _ => ShowSelectionSummary();
        virtualList.OnBindItem(BindListItem).FillSize();

        actionButton.Click += _ =>
        {
            dialog.Title = "Demo dialog form";
            dialog.Body = SelectionSummary();
            dialog.Show();
        };
        foundationsToggleButton.Click += _ =>
        {
            foundationsScopeEnabled = !foundationsScopeEnabled;
            foundationsScopedFocused &= foundationsScopeEnabled;
            ShowFoundations();
        };
        foundationsScopedButton.Click += _ =>
        {
            if (foundationsScopeEnabled)
            {
                ++foundationsActionCount;
                ShowFoundationsAction();
            }
        };
        foundationsScopedButton.FocusChanged += e =>
        {
            foundationsScopedFocused = e.Focused && foundationsScopeEnabled;
            ShowFoundationsFocus();
        };
        keyTargetBox.FocusChanged += e =>
        {
            keyTargetFocused = e.Focused;
            ShowKeyTarget(Theme.Active.Value);
        };
        keyTargetBox.KeyDown += e =>
        {
            lastKey = e.Key;
            ShowKeyTargetText();
        };

        listPanel = CreateListPanel();
        columns = FlexBox.Row(listPanel, columnGap, CreateMainPanel()).AlignItems(AlignItems.Stretch).FillHeight();
        var page = new PageShell("Home Page - FUI-CS Demo ✌🏼", DemoRoutes.Home, columns, compactMinHeight: CompactPageMinHeight);
        root = new SelectionArea().FillSize().Child(new FlexBox().FillSize().Child(page).Child(dialog));
        Application.OnUnmount(Responsive.Subscribe(ApplyColumnLayout).Dispose);
        Application.OnUnmount(Theme.Bind(theme =>
        {
            root.Background = theme.Colors.Background;
            ShowKeyTarget(theme);
        }).Dispose);
        clock = DemoShell.OnClockTick(ShowElapsed);
        ShowFoundations();
        ShowSelectionSummary();
    }

    public override Node GetRoot() => root;

    public override void Dispose()
    {
        clock.Dispose();
        base.Dispose();
    }

    /// <summary>The list beside the showcase, or above it in the compact layout.</summary>
    private void ApplyColumnLayout(bool compact)
    {
        columns.FlexDirection = compact ? FlexDirection.Column : FlexDirection.Row;
        if (compact)
        {
            listPanel.FillWidth().Height(CompactListPanelHeight);
            columnGap.Width(0.0f).Height(12.0f);
        }
        else
        {
            listPanel.Width(250.0f).FillHeight();
            columnGap.Width(24.0f).Height(Length.Auto);
        }
    }

    private Panel CreateListPanel() => new Panel()
        .Child(new DemoText("Virtual List", DemoTextStyle.Heading2))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(new DemoText("This list contains 100,000 items. Scroll to see more.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(virtualList);

    private Panel CreateMainPanel() => new Panel()
        .Child(new DemoText("Basic controls showcase", DemoTextStyle.Heading2))
        .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
        .Child(new DemoText("Demo controls are wrapped by route design-system subclasses.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyTightGap))
        .Child(new DemoNavLink(DemoRoutes.Source, "View the FUI-CS demo source code on GitHub"))
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(elapsedText)
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(actionButton)
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(summaryText)
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoScrollBox("ControlsScrollBox")
            .FitWidthWhenCompact()
            .FillSize()
            .Child(CreateCommonControlsPanel())
            .Child(Spacing.Vertical(Spacing.PanelSectionGap))
            .Child(CreateImagesPanel())
            .Child(Spacing.Vertical(Spacing.PanelSectionGap))
            .Child(CreateNestedScrollPanel())
            .Child(Spacing.Vertical(Spacing.PanelSectionGap))
            .Child(CreateFoundationsPanel()))
        .FillSize();

    private Panel CreateCommonControlsPanel()
    {
        var accessibility = new RichText(
                new Span("Use keyboard Tab to navigate through the controls and inspect the semantic tree to explore accessibility features - FUI-CS is fully ARIA compliant out of the box. Press "),
                new Span(DebugShortcutLabel()).Bold(),
                new Span(" to open the debug dialog when on-requested developer tools are enabled; apps can enable or disable this surface through runtime configuration."))
            .FontSize(14.0f)
            .FillWidth();
        Application.OnUnmount(Theme.Bind(theme => accessibility.TextColor = theme.Colors.TextMuted).Dispose);
        return new Panel()
            .Child(new DemoText("Common Controls", DemoTextStyle.Heading3))
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(accessibility)
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(new DemoText("Dropdown", DemoTextStyle.Label))
            .Child(Spacing.Vertical(Spacing.LabelToControlGap))
            .Child(dropdown)
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(new DemoText("Checkboxes", DemoTextStyle.Label))
            .Child(Spacing.Vertical(Spacing.LabelToControlGap))
            .Child(triStateCheckbox)
            .Child(Spacing.Vertical(Spacing.MicroStackGap))
            .Child(biStateCheckbox)
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(new DemoText("Radio group", DemoTextStyle.Label))
            .Child(Spacing.Vertical(Spacing.LabelToControlGap))
            .Child(qualityRadioGroup)
            .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
            .Child(new DemoText("Sliders", DemoTextStyle.Label))
            .Child(Spacing.Vertical(Spacing.LabelToControlGap))
            .Child(FlexBox.Row(
                    FlexBox.Column(new DemoText("Horizontal", DemoTextStyle.BodySecondary), Spacing.Vertical(Spacing.MicroStackGap), horizontalSlider),
                    Spacing.Horizontal(16.0f),
                    FlexBox.Column(new DemoText("Vertical", DemoTextStyle.BodySecondary), Spacing.Vertical(Spacing.MicroStackGap), verticalSlider))
                .AlignItems(AlignItems.Stretch)
                .FlexWrap(FlexWrap.Wrap))
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Width(Length.Auto);
    }

    private Panel CreateImagesPanel() => new Panel()
        .Child(new DemoText("Images with Transparency", DemoTextStyle.Heading3))
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoText("Image 1: JPG with no transparency", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.LabelToControlGap))
        .Child(Image.Load("https://upload.wikimedia.org/wikipedia/commons/d/d3/Golden_Gate_Bridge_at_sunset_1.jpg", ObjectFit.Cover)
            .AltText("Sample image1 - JPG")
            .Width(640.0f)
            .Height(480.0f)
            .ToolTip(DelayedToolTip("Sample image1 - JPG")))
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoText("Image 2: PNG with transparency", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.LabelToControlGap))
        .Child(Image.Load("https://upload.wikimedia.org/wikipedia/commons/6/67/1932_eagle_reverse%28Transparency%29.png", ObjectFit.Cover)
            .AltText("Sample image2 - PNG with transparency")
            .ToolTip(DelayedToolTip("Sample image2 - PNG with transparency")))
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoText("Image 3: SVG", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.LabelToControlGap))
        .Child(Svg.Load("https://upload.wikimedia.org/wikipedia/commons/d/d0/Drawsvgbird.svg")
            .AltText("Sample SVG image")
            .Height(600.0f)
            .ToolTip(DelayedToolTip("Sample SVG image")))
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoText("Image 4: Custom bitmap", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.LabelToControlGap))
        .Child(new Image(bitmap.TextureId, ObjectFit.Contain)
            .AltText("App-owned premultiplied RGBA bitmap sample")
            .ToolTip(DelayedToolTip("App-owned premultiplied RGBA bitmap sample")));

    private static Panel CreateNestedScrollPanel()
    {
        var content = new FlexBox()
            .Width(800.0f)
            .Padding(16.0f, 16.0f, 16.0f, 16.0f)
            .Child(FlexBox.Column(
                new DemoText("Nested Scroll Content - START", DemoTextStyle.Label),
                Spacing.Vertical(72.0f),
                new DemoText("Lorem Ipsum is simply dummy text of the printing and typesetting industry. Lorem Ipsum has been the industry's standard dummy text ever since 1966, when designers at Letraset and James Mosley, the librarian at St Bride Printing Library, took a 1914 Cicero translation and scrambled it to make dummy text for Letraset's Body Type sheets. It has survived not only many decades, but also the leap into electronic typesetting, remaining essentially unchanged. It was popularised thanks to these sheets and more recently with desktop publishing software including versions of Lorem Ipsum.", DemoTextStyle.BodySecondary),
                Spacing.Vertical(116.0f),
                new DemoText("Nested Scroll Content - END", DemoTextStyle.Label)));
        var nested = new DemoScrollBox("Nested ScrollBox")
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Visible,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            ScrollBarGutter = 8.0f,
        };
        nested.Width(300.0f).Height(240.0f).Child(content);
        return new Panel()
            .Child(new DemoText("Nested ScrollBox", DemoTextStyle.Heading3))
            .Child(Spacing.Vertical(Spacing.PanelSectionGap))
            .Child(new Panel().Child(nested).Width(360.0f));
    }

    private Panel CreateFoundationsPanel() => new Panel()
        .Child(new DemoText("Control foundations", DemoTextStyle.Heading3))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyTightGap))
        .Child(new DemoText("Tab onto these buttons to see the focus ring. Use the outer toggle to disable the parent scope and watch the child button dim and stop activating.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(foundationsStatusText)
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(foundationsToggleButton)
        .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
        .Child(foundationsScopeBox)
        .Child(Spacing.Vertical(Spacing.PanelSectionGap))
        .Child(new DemoText("Keyboard focus target", DemoTextStyle.Label))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyTightGap))
        .Child(new DemoText("Tab onto this box, then press keys to verify retained focus and key routing.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
        .Child(keyTargetBox)
        .Width(Length.Auto);

    private static ToolTip DelayedToolTip(string text) => new(text) { InitialShowDelay = TimeSpan.FromSeconds(1) };

    private static void BindListItem(FlexBox container, int index)
    {
        if (container.Children.Count == 0)
        {
            container
                .FillWidth()
                .Padding(16.0f, 8.0f, 8.0f, 8.0f)
                .CornerRadius(12.0f)
                .Child(new DemoText("", DemoTextStyle.Body))
                .Child(new FlexBox().FillWidth().Height(4.0f))
                .Child(new DemoText("", DemoTextStyle.Caption));
        }
        ColorUtils.ApplyRowBackground(container, index, Theme.Active.Value);
        ((Text)container.Children[0]).Text = "Item " + index;
        ((Text)container.Children[2]).Text = "Description for item " + index;
    }

    private void ShowElapsed(int nowSeconds)
    {
        startTick ??= nowSeconds;
        var elapsed = Math.Max(0, nowSeconds - startTick.Value);
        elapsedText.Text = $"Time since app started: {elapsed / 3600:00}:{elapsed % 3600 / 60:00}:{elapsed % 60:00}";
    }

    private void ShowFoundations()
    {
        foundationsStatusText.Text = foundationsScopeEnabled ? "Scoped parent: enabled" : "Scoped parent: disabled via parent container";
        foundationsToggleButton.Label = foundationsScopeEnabled ? "Disable scoped child" : "Enable scoped child";
        foundationsScopeBox.IsEnabled = foundationsScopeEnabled;
        ShowFoundationsFocus();
        ShowFoundationsAction();
    }

    private void ShowFoundationsFocus() =>
        foundationsFocusText.Text = foundationsScopedFocused ? "Scoped child focus: focused" : "Scoped child focus: unfocused";

    private void ShowFoundationsAction() => foundationsActionText.Text = "Scoped child activations " + foundationsActionCount;

    private void ShowKeyTarget(Theme theme)
    {
        var accent = theme.Colors.Accent;
        keyTargetBox.Background = keyTargetFocused ? Color.FromRgba(accent.R, accent.G, accent.B, 0x22) : theme.Colors.Surface;
        keyTargetBox.Border = new Border(1.0f, keyTargetFocused ? theme.Colors.FocusRing : theme.Colors.Border);
        ShowKeyTargetText();
    }

    private void ShowKeyTargetText() =>
        keyTargetText.Text = "Focus me, then press keys. Last key: " + (lastKey.Length > 0 ? lastKey : "none");

    private void ShowSelectionSummary() => summaryText.Text = SelectionSummary();

    private string SelectionSummary()
    {
        var index = dropdown.SelectedIndex;
        var size = index >= 0 && index < dropdownItems.Length ? dropdownItems[index].Label : "none";
        var tri = triStateCheckbox.IsChecked switch
        {
            true => "checked",
            null => "mixed",
            false => "unchecked",
        };
        var bi = biStateCheckbox.IsChecked == true ? "checked" : "unchecked";
        return $"Selections: size={size}, tri={tri}, bi={bi}, radio={qualityRadioGroup.SelectedValue}, "
            + $"horizontal={Math.Round(horizontalSlider.Value)}, vertical={Math.Round(verticalSlider.Value)}";
    }

    private static string DebugShortcutLabel() => Platform.Family switch
    {
        PlatformFamily.Apple => "Cmd+Shift+F12",
        PlatformFamily.Windows => "Win+Shift+F12",
        PlatformFamily.Linux => "Super+Shift+F12",
        _ => "Meta+Shift+F12",
    };
}
