using System.Globalization;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Advanced.Reorder;

/// <summary>
/// One slot of the reorder list: an insertion marker above a card with a drag grip. The slot is the drop target for
/// its index; the grip, and the glyph on it that receives the pointer, start a drag of the item the slot shows. While
/// its item is dragged the slot collapses, and the section's drag preview stands in for the card.
/// </summary>
internal sealed class ReorderRowView
{
    private const string GripGlyph = "☰";

    private readonly ReorderDemoSection section;
    private ReorderDemoItem? currentItem;
    private string? pendingDragItemId;

    public ReorderRowView(ReorderDemoSection section, int rawIndex)
    {
        this.section = section;
        RawIndex = rawIndex;
        Marker = new FlexBox()
            .FillWidth()
            .Height(ReorderDemoLogic.MarkerHeight)
            .CornerRadius(ReorderDemoLogic.MarkerHeight * 0.5f)
            .Opacity(0.0f);
        GripLabel = CreateGripLabel()
            .Cursor(CursorStyle.Grab)
            .DragData(ProvideDragData)
            .DragAllowedEffects(DragDropEffects.Move)
            .OnDragCompleted(e => HandleDragCompleted(e.Effect));
        Grip = CreateGrip(GripLabel)
            .Cursor(CursorStyle.Grab)
            .SemanticRole(SemanticRole.Button)
            .SemanticLabel("Drag grip")
            .DragData(ProvideDragData)
            .DragAllowedEffects(DragDropEffects.Move)
            .OnDragCompleted(e => HandleDragCompleted(e.Effect));
        TitleText = new DemoText("", DemoTextStyle.Body);
        DetailText = new DemoText("", DemoTextStyle.Caption).MaxLines(2).Overflow(TextOverflow.Ellipsis);
        Card = CreateCard(section.RowBodyHeight, Grip, TitleText, DetailText);
        // Clipped at the card's border, so text a narrow screen wraps onto more lines stays inside the card.
        Slot = FlexBox.Column(Marker, Card)
            .FillWidth()
            .ClipToBounds(true)
            .AllowDrop()
            .OnDragEnter(e => e.Proposal = section.PreviewInsertion(e, RawIndex))
            .OnDragOver(e => e.Proposal = section.PreviewInsertion(e, RawIndex))
            .OnDragLeave(section.HandleTargetLeave)
            .OnDrop(section.DropAtPreview);
    }

    public int RawIndex { get; }

    public FlexBox Marker { get; }

    public Text GripLabel { get; }

    public FlexBox Grip { get; }

    public Text TitleText { get; }

    public Text DetailText { get; }

    public FlexBox Card { get; }

    public FlexBox Slot { get; }

    /// <summary>The grip's glyph; the drag preview shows one too, so it is not selectable.</summary>
    internal static Text CreateGripLabel() => new DemoText(GripGlyph, DemoTextStyle.Body).Selectable(false);

    /// <summary>The box that centers the grip's glyph at the card's leading edge.</summary>
    internal static FlexBox CreateGrip(Text label) => new FlexBox()
        .Width(36.0f)
        .Height(40.0f)
        .JustifyContent(JustifyContent.Center)
        .AlignItems(AlignItems.Center)
        .Child(label);

    /// <summary>A card of <paramref name="height"/>: the grip, then the title over the detail.</summary>
    internal static FlexBox CreateCard(float height, FlexBox grip, Text title, Text detail) => new FlexBox()
        .FillWidth()
        .Height(height)
        .Padding(16.0f, 14.0f, 16.0f, 14.0f)
        .CornerRadius(18.0f)
        .JustifyContent(JustifyContent.Center)
        .Child(FlexBox.Row(
                grip,
                new FlexBox().Width(8.0f).Height(1.0f),
                FlexBox.Column(title, new FlexBox().FillWidth().Height(4.0f), detail).FillWidth())
            .FillWidth());

    public void BindItem(ReorderDemoItem item)
    {
        currentItem = item;
        TitleText.Text = item.Label;
        DetailText.Text = item.Detail;
        Grip.SemanticLabel = "Drag grip for " + item.Label;
        Card.SemanticRole(SemanticRole.StaticText)
            .SemanticLabel("Reorder item " + (RawIndex + 1).ToString(CultureInfo.InvariantCulture) + ": " + item.Label);
    }

    public void ApplyTheme(Theme theme, string? activeSourceId, int rawInsertionIndex)
    {
        var colors = theme.Colors;
        var isSource = currentItem is not null && currentItem.Id == activeSourceId;
        var visibility = isSource ? Visibility.Collapsed : Visibility.Normal;
        Marker.Background(colors.Accent).Opacity(rawInsertionIndex == RawIndex ? 1.0f : 0.0f).Visibility(visibility);
        Card.Visibility(visibility);
        if (!isSource)
        {
            ColorUtils.ApplyRowBackground(Card, RawIndex, theme);
        }
        Card.Border(1.0f, isSource ? colors.Accent : colors.Border);
        TitleText.TextColor(isSource ? colors.Surface : colors.TextPrimary);
        DetailText.TextColor(isSource ? colors.Surface : colors.TextMuted);
        var cursor = isSource ? CursorStyle.Grabbing : CursorStyle.Grab;
        Grip.Cursor(cursor);
        GripLabel.TextColor(isSource ? colors.Surface : colors.TextPrimary).Cursor(cursor);
    }

    private DragDataObject? ProvideDragData()
    {
        if (currentItem is null)
        {
            return null;
        }
        pendingDragItemId = currentItem.Id;
        return section.BeginDrag(currentItem);
    }

    private void HandleDragCompleted(DragDropEffects effect)
    {
        var draggedItemId = pendingDragItemId;
        pendingDragItemId = null;
        if (draggedItemId is not null)
        {
            section.CompleteDrag(draggedItemId, effect);
        }
    }
}
