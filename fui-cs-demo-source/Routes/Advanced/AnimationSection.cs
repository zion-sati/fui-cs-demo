using System;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Advanced;

/// <summary>
/// A preview card whose background and opacity transition between a calm and an emphasized state, and a scroll box of
/// eighteen rows with a logical tail beyond them, scrolled smoothly to set targets.
/// </summary>
internal sealed class AnimationSection
{
    private const float RowHeight = 80.0f;
    private const int RowCount = 18;
    private const float ViewportHeight = 280.0f;
    private const float LogicalTail = 240.0f;
    private const float LogicalContentHeight = (RowCount * RowHeight) + LogicalTail;
    private const string CalmBody = "Opacity and background transitions stay on the same retained node while the control layer keeps behavior ownership elsewhere.";

    private static readonly AnimationTiming ScrollTiming = new(TimeSpan.FromMilliseconds(300.0), Easings.CubicOut);

    private readonly DemoText previewTitle = new("Calm transition target", DemoTextStyle.Heading3);
    private readonly DemoText previewBody = new(CalmBody, DemoTextStyle.BodySecondary);
    private readonly FlexBox previewCard;
    private readonly FlexBox[] rows = new FlexBox[RowCount];
    private readonly DemoScrollBox scrollBox;
    private readonly DemoText previewStatus = new("Preview: calm", DemoTextStyle.Body);
    private readonly DemoText scrollStatus = new("ScrollBox idle", DemoTextStyle.BodySecondary);
    private bool emphasized;

    public AnimationSection()
    {
        previewCard = new FlexBox()
            .FillWidth()
            .MinHeight(128.0f)
            .Padding(18.0f, 18.0f, 18.0f, 18.0f)
            .CornerRadius(20.0f)
            .ClipToBounds(true)
            .Child(FlexBox.Column(previewTitle, Spacing.Vertical(8.0f), previewBody).FillWidth())
            .Transitions(new NodeTransitions
            {
                Background = new AnimationTiming(TimeSpan.FromMilliseconds(1000.0), Easings.CubicOut),
                Opacity = new AnimationTiming(TimeSpan.FromMilliseconds(500.0), Easings.CubicOut),
            });
        var content = new FlexBox().FillWidth();
        for (var index = 0; index < RowCount; ++index)
        {
            var detail = index == RowCount - 1
                ? "The final target proves retained smooth scrolling can drive to the far end of the viewport."
                : "Retained content stays pooled and composable while the viewport animates independently.";
            rows[index] = new FlexBox()
                .FillWidth()
                .Height(RowHeight)
                .Padding(16.0f, 12.0f, 16.0f, 12.0f)
                .CornerRadius(14.0f)
                .ClipToBounds(true)
                .Child(FlexBox.Column(
                        new DemoText("Animation sample row " + (index + 1), DemoTextStyle.Body),
                        Spacing.Vertical(4.0f),
                        // One line, so a phone's narrower row keeps the fixed height the scroll targets count in.
                        new DemoText(detail, DemoTextStyle.BodySecondary).MaxLines(1).Overflow(TextOverflow.Ellipsis))
                    .FillWidth());
            content.Child(rows[index]);
        }
        scrollBox = new DemoScrollBox("advanced:animation-scroll")
        {
            IsHorizontalScrollEnabled = false,
            VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Hidden,
            ScrollBarGutter = 8.0f,
        };
        scrollBox.ScrollContentSize(null, LogicalContentHeight).FillWidth().Height(ViewportHeight).Child(content);
        Application.OnUnmount(Theme.Bind(ApplyTheme).Dispose);
    }

    public Node[] BuildTransitions() =>
    [
        new DemoText("Transitions", DemoTextStyle.Heading3),
        Spacing.Vertical(Spacing.TitleToSupportingGap),
        previewCard,
        Spacing.Vertical(14.0f),
        FlexBox.Row(
                new DemoButton("Set calm preview").Width(170.0f).Height(60.0f).OnClick(_ => SetEmphasized(false)),
                Spacing.Horizontal(12.0f),
                new DemoButton("Emphasize preview card", DemoButtonTone.Primary).Width(190.0f).Height(60.0f).OnClick(_ => SetEmphasized(true)))
            .FillWidth()
            .FlexWrap(FlexWrap.Wrap)
            .AlignItems(AlignItems.Stretch),
        Spacing.Vertical(14.0f),
        previewStatus,
    ];

    public Node[] BuildScrollSurfaces() =>
    [
        new DemoText("Scroll Surfaces", DemoTextStyle.Heading3),
        Spacing.Vertical(Spacing.TitleToSupportingGap),
        new Panel().Child(scrollBox.Width(700.0f).MaxWidth(100.0f, Unit.Percent)),
        Spacing.Vertical(14.0f),
        FlexBox.Row(
                ScrollButton("Scroll to first sample", 180.0f, DemoButtonTone.Secondary, 0.0f, "first sample"),
                Spacing.Horizontal(12.0f),
                ScrollButton("Scroll to 8th sample", 190.0f, DemoButtonTone.Secondary, 7.0f * RowHeight, "8th sample"),
                Spacing.Horizontal(12.0f),
                ScrollButton("Scroll to 14th sample", 180.0f, DemoButtonTone.Primary, 13.0f * RowHeight, "14th sample"))
            .FillWidth()
            .FlexWrap(FlexWrap.Wrap)
            .AlignItems(AlignItems.Stretch),
        Spacing.Vertical(10.0f),
        FlexBox.Row(ScrollButton("Scroll to logical tail", 170.0f, DemoButtonTone.Secondary, LogicalContentHeight - ViewportHeight, "logical tail"))
            .FillWidth()
            .FlexWrap(FlexWrap.Wrap),
        Spacing.Vertical(10.0f),
        scrollStatus,
        Spacing.Vertical(10.0f),
        new DemoText(
            "Each card transitions independently. Use the scroll buttons to animate the viewport to different positions in the retained content, including the logical tail beyond the last card.",
            DemoTextStyle.BodySecondary),
        Spacing.Vertical(Spacing.PageSectionGap),
    ];

    private DemoButton ScrollButton(string label, float width, DemoButtonTone tone, float offsetY, string target) =>
        new DemoButton(label, tone).Width(width).Height(60.0f).OnClick(_ =>
        {
            scrollBox.ScrollToAnimated(0.0f, offsetY, ScrollTiming);
            scrollStatus.Text = "Scrolling to " + target + "...";
        });

    private void SetEmphasized(bool value)
    {
        emphasized = value;
        previewTitle.Text = value ? "Emphasized transition target" : "Calm transition target";
        previewBody.Text = value
            ? "The preview card now drives both opacity and background transitions together from one typed slot set."
            : CalmBody;
        previewStatus.Text = value ? "Preview: emphasized" : "Preview: calm";
        ApplyTheme(Theme.Active.Value);
    }

    private void ApplyTheme(Theme theme)
    {
        previewCard.Background = emphasized ? theme.Colors.AccentHovered : theme.Colors.Surface;
        previewCard.Opacity = emphasized ? 1.0f : 0.7f;
        previewCard.Border = new Border(1.0f, theme.Colors.Border);
        for (var index = 0; index < rows.Length; ++index)
        {
            ColorUtils.ApplyRowBackground(rows[index], index, theme);
        }
    }
}
