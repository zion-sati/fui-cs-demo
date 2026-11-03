using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Advanced.Reorder;

/// <summary>
/// Drag-and-drop reorder of retained rows inside an inner <see cref="ScrollBox"/>: insertion markers, a drag preview
/// that copies the dragged card and follows the pointer, cursor feedback, and auto-scroll while the pointer rests near
/// the viewport's edge. The drag manager re-evaluates drop targets only on pointer events, so auto-scroll runs on a
/// timer and moves the insertion preview itself while the list scrolls under a still pointer.
/// </summary>
internal sealed class ReorderDemoSection : IDisposable
{
    private const float PreviewWidth = 272.0f;
    private const float PreviewOffsetX = 2.0f;
    private const float PreviewOffsetY = 2.0f;
    private const float PreviewMargin = 12.0f;
    private static readonly TimeSpan AutoScrollDelay = TimeSpan.FromMilliseconds(16);

    private readonly List<ReorderRowView> rows = [];
    private readonly List<ReorderDemoItem> items = ReorderDemoLogic.CreateItems();
    private readonly ScrollBox scrollBox;
    private readonly Text orderStatusText = new DemoText("", DemoTextStyle.Body).NodeId("reorder-order");
    private readonly Text dragStatusText = new DemoText("", DemoTextStyle.Caption).NodeId("reorder-drag-status");
    private readonly Text viewportStatusText = new DemoText("", DemoTextStyle.Caption).NodeId("reorder-viewport-status");
    // The preview's text is not selectable, so the pointer passes through it to the rows: the preview trails the pointer
    // by a frame, and a fast move would otherwise land on its text and lose the drop target until the next move.
    private readonly Text previewGripText = ReorderRowView.CreateGripLabel();
    private readonly Text previewTitleText = new DemoText("", DemoTextStyle.Body).Selectable(false);
    private readonly Text previewDetailText = new DemoText("", DemoTextStyle.Caption).Selectable(false).MaxLines(2).Overflow(TextOverflow.Ellipsis);
    private readonly FlexBox previewGhost;
    private readonly FlexBox endMarker = new FlexBox()
        .FillWidth()
        .Height(ReorderDemoLogic.MarkerHeight)
        .CornerRadius(ReorderDemoLogic.MarkerHeight * 0.5f)
        .Opacity(0.0f);
    private readonly Text endDropText = new DemoText("Drop at end of reorder list", DemoTextStyle.Caption);
    private readonly FlexBox endDropZone;
    private readonly FlexBox sectionRoot;
    private Theme theme = Theme.Active.Value;
    private string? activeDragItemId;
    private int rawInsertionIndex = -1;
    private float autoScrollDeltaY;
    private string dragStatusMessage = "idle";
    private float previewPointerX = float.NaN;
    private float previewPointerY = float.NaN;
    private float previewWidth;
    private ITimer? autoScrollTimer;
    private readonly List<IDisposable> subscriptions = [];

    /// <summary>The height of each row's card: taller in the compact layout, where a phone wraps their text.</summary>
    public float RowBodyHeight { get; private set; } = ReorderDemoLogic.RowBodyHeightFor(Responsive.IsCompact);

    public ReorderDemoSection()
    {
        var scrollContent = FlexBox.Column().FillWidth();
        scrollBox = new ScrollBox()
            .NodeId("reorder-viewport")
            .ScrollBarGutter(8.0f)
            .HorizontalScrollEnabled(false)
            .VerticalScrollEnabled(true)
            .VerticalScrollBarVisibility(ScrollBarVisibility.Visible)
            .HorizontalScrollBarVisibility(ScrollBarVisibility.Hidden)
            .PersistScroll(false)
            .FillWidth()
            .Height(ReorderDemoLogic.ViewportHeight)
            .SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder demo viewport");
        scrollBox.VerticalScrollBar.TrackCornerRadius(8.0f).ThumbCornerRadius(8.0f);
        scrollBox.Child(scrollContent);

        for (var index = 0; index < items.Count; ++index)
        {
            var row = new ReorderRowView(this, index);
            row.BindItem(items[index]);
            rows.Add(row);
            scrollContent.Child(row.Slot);
        }

        endDropZone = new FlexBox()
            .FillWidth()
            .Height(ReorderDemoLogic.EndSlotHeight)
            .AllowDrop()
            .OnDragEnter(e => e.Proposal = PreviewInsertion(e, items.Count))
            .OnDragOver(e => e.Proposal = PreviewInsertion(e, items.Count))
            .OnDragLeave(HandleTargetLeave)
            .OnDrop(DropAtPreview)
            .Child(endMarker)
            .Child(new FlexBox()
                .FillWidth()
                .Height(36.0f)
                .CornerRadius(14.0f)
                .JustifyContent(JustifyContent.Center)
                .AlignItems(AlignItems.Center)
                .Child(endDropText));
        scrollContent.Child(endDropZone);

        previewGhost = ReorderRowView.CreateCard(RowBodyHeight, ReorderRowView.CreateGrip(previewGripText), previewTitleText, previewDetailText)
            .PositionAbsolute()
            .Width(PreviewWidth)
            .Opacity(0.0f)
            .Visibility(Visibility.Hidden)
            .SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder drag preview");
        var previewPortal = new Portal().PositionAbsolute().Position(0.0f, 0.0f).FillSize().Child(previewGhost);

        var body = FlexBox.Column(
                scrollBox,
                Spacer(14.0f),
                orderStatusText,
                Spacer(6.0f),
                dragStatusText,
                Spacer(6.0f),
                viewportStatusText,
                Spacer(10.0f))
            .Width(600.0f)
            .MaxWidth(100.0f, Unit.Percent);
        sectionRoot = new FlexBox().NodeId("reorder-section").FillWidth().Child(body).Child(previewPortal);

        subscriptions.Add(Responsive.Subscribe(ApplyRowBodyHeight));
        subscriptions.Add(scrollBox.ScrollState.OffsetY.Subscribe(_ => SyncViewportStatus()));
        subscriptions.Add(scrollBox.ScrollState.ViewportHeight.Subscribe(_ => SyncViewportStatus()));
        SyncAll();
        subscriptions.Add(Theme.Active.Subscribe(next =>
        {
            theme = next;
            ApplyTheme();
        }));
    }

    /// <summary>Stops the edge auto-scroll and releases the section's subscriptions, when its route is disposed.</summary>
    public void Dispose()
    {
        StopAutoScroll();
        autoScrollTimer?.Dispose();
        autoScrollTimer = null;
        foreach (var subscription in subscriptions)
        {
            subscription.Dispose();
        }
        subscriptions.Clear();
    }

    /// <summary>The reorder list with its status lines.</summary>
    public FlexBox Root => sectionRoot;

    /// <summary>The last drag event, for under the section.</summary>
    public Text StatusText { get; } = new DemoText("", DemoTextStyle.Caption).NodeId("reorder-status");

    /// <summary>How to drag, for under the status.</summary>
    public Text HintText { get; } = new DemoText(
            "Drag a grip with a mouse, or touch and hold before moving. Release to drop. The grip highlights the source row and drop indicators mark the insertion point.",
            DemoTextStyle.BodySecondary)
        .MaxLines(4);

    internal DragDataObject BeginDrag(ReorderDemoItem item)
    {
        SelectionArea.ClearCurrentSelection();
        activeDragItemId = item.Id;
        var sourceIndex = ReorderDemoLogic.FindItemIndex(items, item.Id);
        rawInsertionIndex = sourceIndex >= 0 ? sourceIndex : -1;
        ClearPreview();
        // The preview copies the card, at the width it has before its slot collapses.
        previewWidth = sourceIndex >= 0 ? rows[sourceIndex].Card.GetBounds().Width : 0.0f;
        dragStatusMessage = "dragging " + item.Label;
        StatusText.Text = "Dragging: " + item.Label;
        SyncAll();
        return new DragDataObject().SetFormat(ReorderDemoLogic.DragFormat, item.Id).SetText(item.Label);
    }

    internal void CompleteDrag(string itemId, DragDropEffects effect)
    {
        var itemLabel = FindItem(itemId)?.Label ?? "item";
        activeDragItemId = null;
        rawInsertionIndex = -1;
        ClearPreview();
        StopAutoScroll();
        if (effect == DragDropEffects.Move)
        {
            var slot = Slot(ReorderDemoLogic.FindItemIndex(items, itemId));
            dragStatusMessage = "moved " + itemLabel + " to slot " + slot;
            StatusText.Text = "Moved " + itemLabel + " to position " + slot;
        }
        else
        {
            dragStatusMessage = "cancelled " + itemLabel;
            StatusText.Text = "Drag cancelled";
        }
        SyncAll();
    }

    /// <summary>Previews the drop at <paramref name="insertionIndex"/> and accepts it as a move.</summary>
    internal DropProposal PreviewInsertion(DragEventArgs args, int insertionIndex)
    {
        var itemId = DraggedItemId(args);
        var sourceIndex = itemId is null ? -1 : ReorderDemoLogic.FindItemIndex(items, itemId);
        if (itemId is null || sourceIndex < 0)
        {
            StopAutoScroll();
            return DropProposal.None;
        }
        activeDragItemId = itemId;
        rawInsertionIndex = insertionIndex;
        SetAutoScrollDelta(PointerAutoScrollDelta(args.Y));
        var normalizedIndex = ReorderDemoLogic.NormalizeInsertionIndex(sourceIndex, insertionIndex, items.Count);
        previewPointerX = args.X;
        previewPointerY = args.Y;
        dragStatusMessage = "preview slot " + Slot(normalizedIndex);
        StatusText.Text = "Insert at position " + Slot(normalizedIndex);
        SyncAll();
        return new DropProposal(DragDropEffects.Move, ShowInsertionMarker: true);
    }

    /// <summary>
    /// The pointer left a slot: near the viewport's edge the preview moves to the edge the list scrolls toward;
    /// elsewhere the drag has no drop position until it reaches another slot.
    /// </summary>
    internal void HandleTargetLeave(DragEventArgs args)
    {
        if (activeDragItemId is null)
        {
            return;
        }
        previewPointerX = args.X;
        previewPointerY = args.Y;
        var visibleRange = VisibleRange();
        SetAutoScrollDelta(PointerAutoScrollDelta(args.Y));
        if (autoScrollDeltaY == 0.0f)
        {
            rawInsertionIndex = -1;
            dragStatusMessage = "dragging " + ActiveDragLabel();
        }
        else
        {
            var direction = autoScrollDeltaY < 0.0f ? -1 : 1;
            var insertionIndex = ReorderDemoLogic.EdgeInsertionIndex(direction, items.Count, visibleRange);
            if (insertionIndex >= 0)
            {
                rawInsertionIndex = insertionIndex;
                dragStatusMessage = "autoscroll " + (direction < 0 ? "up" : "down");
            }
        }
        SyncAll();
    }

    internal void DropAtPreview(DragEventArgs args)
    {
        var itemId = DraggedItemId(args);
        if (itemId is null || rawInsertionIndex < 0)
        {
            return;
        }
        var normalizedIndex = ReorderDemoLogic.NormalizeInsertionIndex(ReorderDemoLogic.FindItemIndex(items, itemId), rawInsertionIndex, items.Count);
        if (ReorderDemoLogic.MoveItem(items, itemId, rawInsertionIndex))
        {
            for (var index = 0; index < rows.Count; ++index)
            {
                rows[index].BindItem(items[index]);
            }
            StatusText.Text = "Moved to position " + Slot(normalizedIndex);
        }
        rawInsertionIndex = -1;
        SyncAll();
    }

    private static FlexBox Spacer(float height) => new FlexBox().FillWidth().Height(height);

    private static string Slot(int index) => (index + 1).ToString(CultureInfo.InvariantCulture);

    private static string? DraggedItemId(DragEventArgs args) => args.Session.Data.GetFormat(ReorderDemoLogic.DragFormat);

    private void HandleAutoScrollTimer()
    {
        if (activeDragItemId is null || autoScrollDeltaY == 0.0f)
        {
            StopAutoScroll();
            return;
        }
        var currentOffset = scrollBox.ScrollState.OffsetY.Value;
        var nextOffset = ReorderDemoLogic.NextAutoScrollOffset(currentOffset, autoScrollDeltaY, items.Count, ViewportHeight(), RowBodyHeight);
        if (nextOffset == currentOffset)
        {
            StopAutoScroll();
            return;
        }
        scrollBox.SetScrollOffset(0.0f, nextOffset);
        var direction = autoScrollDeltaY < 0.0f ? -1 : 1;
        rawInsertionIndex = ReorderDemoLogic.EdgeInsertionIndex(direction, items.Count, VisibleRange());
        dragStatusMessage = "auto-scrolling " + (direction < 0 ? "up" : "down");
        SyncAll();
        ArmAutoScrollTimer();
    }

    private void ClearPreview()
    {
        previewPointerX = float.NaN;
        previewPointerY = float.NaN;
    }

    private string OrderSummary() => "Order: " + string.Join(", ", items.ConvertAll(item => item.Id));

    private string ActiveDragLabel() => FindItem(activeDragItemId)?.Label ?? "item";

    private ReorderDemoItem? FindItem(string? itemId)
    {
        var index = itemId is null ? -1 : ReorderDemoLogic.FindItemIndex(items, itemId);
        return index < 0 ? null : items[index];
    }

    private void SyncAll()
    {
        orderStatusText.Text = OrderSummary();
        dragStatusText.Text = "Drag status: " + dragStatusMessage;
        SyncViewportStatus();
        SyncPreviewGhost();
        ApplyTheme();
    }

    private void SyncViewportStatus()
    {
        var contentHeight = ReorderDemoLogic.ContentHeight(items.Count, RowBodyHeight);
        viewportStatusText.Text = "Viewport: offset=" + Pixels(scrollBox.ScrollState.OffsetY.Value) + " viewH="
            + Pixels(scrollBox.ScrollState.ViewportHeight.Value) + " content=" + Pixels(contentHeight);
    }

    private static string Pixels(float value) => value.ToString("0.0#####", CultureInfo.InvariantCulture);

    /// <summary>Shows the drag preview, the size of the dragged card, just below and right of the pointer.</summary>
    private void SyncPreviewGhost()
    {
        var item = FindItem(activeDragItemId);
        if (item is null || float.IsNaN(previewPointerX) || float.IsNaN(previewPointerY))
        {
            previewGhost.Visibility(Visibility.Hidden).Opacity(0.0f);
            return;
        }
        previewTitleText.Text = item.Label;
        previewDetailText.Text = item.Detail;
        previewGhost.SemanticLabel = "Reorder drag preview for " + item.Label;
        var width = previewWidth > 0.0f ? previewWidth : PreviewWidth;
        var pointer = sectionRoot.AbsoluteToLocalPosition(previewPointerX, previewPointerY);
        previewGhost
            .Width(width)
            .Height(RowBodyHeight)
            .Position(Math.Clamp(pointer.X + PreviewOffsetX, PreviewMargin, Math.Max(PreviewMargin, width)), Math.Max(PreviewMargin, pointer.Y + PreviewOffsetY))
            .Visibility(Visibility.Normal)
            .Opacity(0.85f);
    }

    private void ApplyTheme()
    {
        var colors = theme.Colors;
        foreach (var row in rows)
        {
            row.ApplyTheme(theme, activeDragItemId, rawInsertionIndex);
        }
        endMarker.Background(colors.Accent).Opacity(rawInsertionIndex == items.Count ? 1.0f : 0.0f);
        endDropZone.Background(Color.FromRgba(colors.Accent.R, colors.Accent.G, colors.Accent.B, 0x08)).Border(1.0f, colors.Border);
        endDropText.TextColor(colors.TextMuted);
        orderStatusText.TextColor(colors.TextPrimary);
        dragStatusText.TextColor(colors.TextMuted);
        viewportStatusText.TextColor(colors.TextMuted);
        previewGhost.Background(colors.Surface).Border(1.0f, colors.Accent);
        previewGripText.TextColor(colors.TextPrimary);
        previewTitleText.TextColor(colors.TextPrimary);
        previewDetailText.TextColor(colors.TextMuted);
    }

    private ReorderVisibleRange VisibleRange() =>
        ReorderDemoLogic.VisibleRange(items.Count, scrollBox.ScrollState.OffsetY.Value, ViewportHeight(), RowBodyHeight);

    private void ApplyRowBodyHeight(bool compact)
    {
        var height = ReorderDemoLogic.RowBodyHeightFor(compact);
        if (height == RowBodyHeight)
        {
            return;
        }
        RowBodyHeight = height;
        foreach (var row in rows)
        {
            row.Card.Height(height);
        }
        SyncViewportStatus();
    }

    private float ViewportHeight()
    {
        var current = scrollBox.ScrollState.ViewportHeight.Value;
        return current > 0.0f ? current : ReorderDemoLogic.ViewportHeight;
    }

    private float PointerAutoScrollDelta(float pointerY)
    {
        var bounds = scrollBox.Viewport.GetBounds();
        return ReorderDemoLogic.PointerAutoScrollDelta(pointerY, bounds.Y, bounds.Height);
    }

    /// <summary>
    /// Starts auto-scrolling at <paramref name="nextDeltaY"/> per step, or stops it at zero. A pointer event that keeps
    /// the same speed re-arms the step instead, so the list steps only while the pointer rests.
    /// </summary>
    private void SetAutoScrollDelta(float nextDeltaY)
    {
        if (MathF.Abs(autoScrollDeltaY - nextDeltaY) <= 0.05f)
        {
            if (nextDeltaY != 0.0f)
            {
                ArmAutoScrollTimer();
            }
            return;
        }
        autoScrollDeltaY = nextDeltaY;
        if (nextDeltaY == 0.0f)
        {
            autoScrollTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            return;
        }
        ArmAutoScrollTimer();
    }

    private void ArmAutoScrollTimer()
    {
        if (autoScrollTimer is null)
        {
            autoScrollTimer = TimeProvider.System.CreateTimer(_ => HandleAutoScrollTimer(), null, AutoScrollDelay, Timeout.InfiniteTimeSpan);
            return;
        }
        autoScrollTimer.Change(AutoScrollDelay, Timeout.InfiniteTimeSpan);
    }

    private void StopAutoScroll()
    {
        autoScrollDeltaY = 0.0f;
        autoScrollTimer?.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
    }
}
