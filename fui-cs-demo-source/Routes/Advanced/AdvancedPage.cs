using FuiCs;
using FuiCsDemo.Advanced.Reorder;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Advanced;

/// <summary>The route's page: one panel per sample in a scroll box under the route's heading.</summary>
internal sealed class AdvancedPage : ManagedApplicationController
{
    private readonly AnimationSection animation = new();
    private readonly WorkerSection worker = new();
    private readonly FetchSection fetch = new();
    private readonly ReorderDemoSection reorder = new();
    private readonly SelectionArea root;

    public AdvancedPage()
    {
        Theme.UseSystem();
        var page = new PageShell("Advanced - FUI-CS Demo", DemoRoutes.Advanced, CreateMainPanel());
        root = new SelectionArea().FillSize().Child(page);
        Application.OnUnmount(Theme.Bind(theme => root.Background = theme.Colors.Background).Dispose);
    }

    public override Node GetRoot() => root;

    public override void Dispose()
    {
        worker.Dispose();
        fetch.Dispose();
        reorder.Dispose();
        base.Dispose();
    }

    private Panel CreateMainPanel() => new Panel()
        .Child(new DemoText("Advanced", DemoTextStyle.Heading2))
        .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
        .Child(new DemoText("Interop demos: animations, worker tasks, scroll surfaces, and browser bridge features.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
        .Child(new DemoText("Each section exercises a cross-cutting runtime capability — use the controls and buttons to drive them live.", DemoTextStyle.BodySecondary))
        .Child(Spacing.Vertical(Spacing.PageSectionGap))
        .Child(new DemoScrollBox("AdvancedScrollBox")
            .FitWidthWhenCompact()
            .FillSize()
            .Child(SectionPanel(animation.BuildTransitions()).FillWidth())
            .Child(SectionPanel(animation.BuildScrollSurfaces()))
            .Child(SectionPanel(worker.Build()).FillWidth())
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Child(SectionPanel(new DemoText("Online Fetch Sample", DemoTextStyle.Heading3), Spacing.Vertical(Spacing.TitleToSupportingGap), fetch.Root).FillWidth())
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Child(SectionPanel(
                    new DemoText("Drag-and-Drop Reorder / Custom Control", DemoTextStyle.Heading3),
                    Spacing.Vertical(Spacing.TitleToSupportingGap),
                    reorder.Root,
                    Spacing.Vertical(12.0f),
                    reorder.StatusText,
                    Spacing.Vertical(10.0f),
                    reorder.HintText)
                .FillWidth()))
        .FillSize();

    private static Panel SectionPanel(params Node[] children)
    {
        var panel = new Panel();
        foreach (var child in children)
        {
            panel.Child(child);
        }
        return panel;
    }
}
