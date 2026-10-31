using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>A pill-shaped route link: the accent while it is the current route or under the pointer.</summary>
public class NavPill : NavLink
{
    private readonly Text label;
    private readonly bool isActive;
    private bool isHovered;

    public NavPill(string href, string label, bool isActive)
        : base(href)
    {
        this.label = new Text(label).FontSize(14.0f).Selectable(false);
        this.isActive = isActive;
        CornerRadius = new CornerRadius(999.0f);
        Padding = new Thickness(16.0f, 8.0f);
        this.Child(this.label);
        SemanticLabel = label;
        InteractionStateChanged += state =>
        {
            isHovered = state.IsHovered || state.IsPressed;
            ApplyDemoTheme(Theme.Active.Value);
        };
        Track(Theme.Bind(ApplyDemoTheme));
    }

    private void ApplyDemoTheme(Theme theme)
    {
        var highlighted = isActive || isHovered;
        Background = highlighted ? theme.Colors.Accent : theme.Colors.Surface;
        label.TextColor = highlighted ? theme.Colors.TextOnAccent : theme.Colors.TextMuted;
    }
}

/// <summary>A text link in the accent, a little brighter under the pointer.</summary>
public class DemoNavLink : NavLink
{
    private readonly Text label;
    private bool isHovered;

    public DemoNavLink(string href, string label, bool openInNewTab = false)
        : base(href, openInNewTab)
    {
        this.label = new Text(label).FontSize(14.0f).Selectable(false);
        this.Child(this.label);
        SemanticLabel = label;
        InteractionStateChanged += state =>
        {
            isHovered = state.IsHovered || state.IsPressed;
            ApplyDemoTheme(Theme.Active.Value);
        };
        Track(Theme.Bind(ApplyDemoTheme));
    }

    private void ApplyDemoTheme(Theme theme) => label.TextColor = isHovered ? theme.Colors.AccentHovered : theme.Colors.Accent;
}

/// <summary>
/// The page title beside the route links; in the compact layout the links wrap on their own line under the title.
/// </summary>
public class NavBar : FlexBox
{
    private const float PillGap = 10.0f;

    private readonly DemoText title;
    private readonly FlexBox links;
    private readonly NavPill[] pills;

    public NavBar(string title, string activeRoute)
    {
        this.title = new DemoText(title, DemoTextStyle.Heading1).VerticalAlign(TextVerticalAlign.Center);
        pills =
        [
            new NavPill(DemoRoutes.Home, "Home", activeRoute == DemoRoutes.Home),
            new NavPill(DemoRoutes.TextFonts, "Text & Fonts", activeRoute == DemoRoutes.TextFonts),
            new NavPill(DemoRoutes.Advanced, "Advanced", activeRoute == DemoRoutes.Advanced),
            new NavPill(DemoRoutes.ImmediateDrawing, "Immediate Drawing", activeRoute == DemoRoutes.ImmediateDrawing),
        ];
        links = FlexBox.Row(pills).FlexWrap(FlexWrap.Wrap);
        FlexDirection = FlexDirection.Row;
        FlexWrap = FlexWrap.Wrap;
        FillWidth = true;
        this.Child(this.title).Child(links);
        Track(Responsive.Subscribe(ApplyLayout));
    }

    private void ApplyLayout(bool compact)
    {
        title.FillWidthPercent = compact ? 100.0f : 50.0f;
        links.FillWidthPercent = compact ? 100.0f : 50.0f;
        links.JustifyContent = compact ? JustifyContent.Start : JustifyContent.End;
        links.Margin = new Thickness(0.0f, compact ? 12.0f : 0.0f, 0.0f, 0.0f);
        // A gap after each pill, and in the compact layout below it too, so wrapped rows stay apart.
        for (var index = 0; index < pills.Length; ++index)
        {
            var last = index == pills.Length - 1;
            pills[index].Margin = new Thickness(0.0f, 0.0f, last ? 0.0f : PillGap, compact ? PillGap : 0.0f);
        }
    }
}

/// <summary>
/// A route's page: its title and links above the content, in a scroll box that fills the window. A wide window keeps a
/// minimum page width and scrolls sideways below it; the compact layout drops it so the page fits a phone. A page
/// whose content stacks there can ask for more height, so the page scrolls instead.
/// </summary>
public class PageShell : DemoScrollBox
{
    private const float MinPageWidth = 800.0f;
    private const float MinPageHeight = 600.0f;

    private readonly FlexBox page;
    private readonly float compactMinHeight;

    public PageShell(string title, string activeRoute, Node content, string scrollBoxName = "mainScrollBox", float compactMinHeight = MinPageHeight)
        : base(scrollBoxName)
    {
        this.compactMinHeight = compactMinHeight;
        page = FlexBox.Column(new NavBar(title, activeRoute), Spacing.Vertical(Spacing.PageSectionGap), content).FillSize();
        this.AddContent(page);
        this.FillSize();
        Track(Responsive.Subscribe(ApplyLayout));
    }

    private void ApplyLayout(bool compact)
    {
        page.MinWidth = Length.Pixels(compact ? 0.0f : MinPageWidth);
        page.MinHeight = Length.Pixels(compact ? compactMinHeight : MinPageHeight);
        page.Padding = new Thickness(compact ? 12.0f : 24.0f);
    }
}
