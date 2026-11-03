using System;
using System.Threading;
using System.Threading.Tasks;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.ImmediateDrawing;

/// <summary>
/// The route's page: eight square drawn cards in a wrapping row, in a panel under the route's heading; they are 300 px
/// wide, or <see cref="Responsive.CompactSampleWidth"/> in the compact layout. Once the page has
/// loaded, a 25 ms tick sweeps a value from 0 to 100 and back and feeds the plots values derived from it, and the paint
/// canvas's hint is rendered from rich text into its bitmap. Disposing the page stops the tick.
/// </summary>
internal sealed class DrawingPage : ManagedApplicationController
{
    private readonly Gauge gauge = new();
    private readonly BarChart chart = new();
    private readonly Waveform wave = new();
    private readonly Sparkline spark = new();
    private readonly PieChart pie = new();
    private readonly ScatterPlot scatter = new();
    private readonly DancingYarn yarn = new();
    private readonly PaintCanvas paintCanvas = new();
    private readonly CancellationTokenSource ticking = new();
    private readonly SelectionArea root;
    private float gaugeValue;
    private float gaugeDirection = 1.0f;

    public DrawingPage()
    {
        Theme.UseSystem();
        // The cards wrap to the panel's width.
        var gallery = new FlexBox()
            .FlexDirection(FlexDirection.Row)
            .FlexWrap(FlexWrap.Wrap)
            .FillWidth()
            .Child(Card(gauge))
            .Child(Card(chart))
            .Child(Card(wave))
            .Child(Card(spark))
            .Child(Card(pie))
            .Child(Card(scatter))
            .Child(Card(yarn))
            .Child(paintCanvas.Margin(0.0f, 0.0f, 0.0f, 16.0f));
        var mainPanel = new Panel()
            .Child(new DemoText("Immediate-mode drawing", DemoTextStyle.Heading2))
            .Child(Spacing.Vertical(Spacing.TitleToSupportingGap))
            .Child(new DemoText(
                "Retained controls can host custom GPU-backed drawing without giving up layout, semantics, input, or theming.",
                DemoTextStyle.BodySecondary))
            .Child(Spacing.Vertical(Spacing.PageSectionGap))
            .Child(new DemoScrollBox("immediate-drawing-content-scroll")
                .FitWidthWhenCompact()
                .FillSize()
                .Child(new Panel()
                    .Child(new DemoText("Live drawing surfaces", DemoTextStyle.Heading3))
                    .Child(Spacing.Vertical(Spacing.HeadingToBodyGap))
                    .Child(new DemoText(
                        "Watch the charts update, pull the dancing yarn, and drag across the paint surface.",
                        DemoTextStyle.BodySecondary))
                    .Child(Spacing.Vertical(Spacing.PageSectionGap))
                    .Child(gallery)
                    .FillWidth()))
            .FillSize();
        var page = new PageShell("Immediate Drawing - FUI-CS Demo", DemoRoutes.ImmediateDrawing, mainPanel, "immediate-drawing-scroll");
        root = new SelectionArea().FillSize().Child(page);
        Application.OnUnmount(Theme.Bind(theme => root.Background = theme.Colors.Background).Dispose);
        Application.OnUnmount(Responsive.Subscribe(ApplySampleSizes).Dispose);
        FrameScheduler.OnLoaded(_ =>
        {
            RunTicks(ticking.Token);
            var hint = new RichText(
                    new Span("Draw ").Color(DrawingColors.Rgba(235, 238, 245, 210)).Italic(),
                    new Span("here").FontSize(30.0f).Color(Color.FromRgb(58, 197, 108)).FontFamily(Theme.Active.Value.Fonts.MonoFamily).Bold().Underline(),
                    new Span(" on this canvas!").Color(DrawingColors.Rgba(235, 238, 245, 210)).Bold())
                .FontFamily(Theme.Active.Value.Fonts.BodyFamily)
                .FontSize(24.0f)
                .Width(268.0f)
                .Height(268.0f);
            hint.Build();
            Bitmap.WhenTextReady(hint, e => paintCanvas.SetHintLabel(e.Node));
        });
    }

    public override Node GetRoot() => root;

    public override void Dispose()
    {
        ticking.Cancel();
        ticking.Dispose();
        base.Dispose();
    }

    private static CustomDrawable Card(CustomDrawable drawable) => drawable.Margin(0.0f, 0.0f, 16.0f, 16.0f);

    /// <summary>The cards fit a phone in the compact layout; each draws to its own size from its next frame.</summary>
    private void ApplySampleSizes(bool compact)
    {
        var size = compact ? Responsive.CompactSampleWidth : 300.0f;
        foreach (var surface in new CustomDrawable[] { gauge, chart, wave, spark, pie, scatter, yarn, paintCanvas })
        {
            surface.Width(size).Height(size);
            surface.InvalidateVisual();
        }
    }

    /// <summary>Ticks every 25 ms, each tick scheduled after the last, until the page is disposed.</summary>
    private async void RunTicks(CancellationToken cancellation)
    {
        try
        {
            while (true)
            {
                await Task.Delay(25, cancellation);
                Tick();
            }
        }
        catch (OperationCanceledException)
        {
            // The page was disposed.
        }
    }

    private void Tick()
    {
        gaugeValue += gaugeDirection * 2.0f;
        if (gaugeValue >= 100.0f)
        {
            gaugeValue = 100.0f;
            gaugeDirection = -1.0f;
        }
        if (gaugeValue <= 0.0f)
        {
            gaugeValue = 0.0f;
            gaugeDirection = 1.0f;
        }
        var value = gaugeValue;
        var sine = (float)Math.Sin(value / 100.0 * Math.PI);
        var cosine = (float)Math.Cos(value / 100.0 * Math.PI * 0.7);
        var distance = Math.Abs(value - 50.0f) * 2.0f;
        gauge.Value.Value = value;
        chart.PushValues(value, distance, (sine * 80.0f) + 20.0f, (cosine * 60.0f) + 40.0f);
        wave.PushValues(value, distance, (sine * 80.0f) + 20.0f, (cosine * 60.0f) + 40.0f);
        spark.Push(value);
        pie.PushValues(value, distance, (sine * 40.0f) + 30.0f, (cosine * 30.0f) + 20.0f);
        scatter.PushValues(
            (float)Math.Sin(value / 100.0 * Math.PI * 2.0),
            (float)Math.Cos(value / 100.0 * Math.PI * 2.0),
            (float)Math.Sin(value / 100.0 * Math.PI * 3.0),
            (float)Math.Sin(value / 100.0 * Math.PI * 1.5));
        yarn.Tick();
    }
}
