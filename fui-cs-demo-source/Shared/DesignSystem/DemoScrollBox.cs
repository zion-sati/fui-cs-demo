using FuiCs;

namespace FuiCsDemo.Shared;

/// <summary>A scroll box on both axes with rounded bars that show while the content overflows.</summary>
public class DemoScrollBox : ScrollBox
{
    private IDisposable? compactBinding;

    public DemoScrollBox(string name)
    {
        NodeId = name;
        IsHorizontalScrollEnabled = true;
        IsVerticalScrollEnabled = true;
        HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
        VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
        foreach (var bar in new[] { HorizontalScrollBar, VerticalScrollBar })
        {
            bar.TrackCornerRadius = 8.0f;
            bar.ThumbCornerRadius = 8.0f;
        }
    }

    /// <summary>In the compact layout the content fits the box's width and scrolls only vertically.</summary>
    public DemoScrollBox FitWidthWhenCompact()
    {
        if (compactBinding is null)
        {
            compactBinding = Responsive.Subscribe(compact => IsHorizontalScrollEnabled = !compact);
            Track(compactBinding);
        }
        return this;
    }
}

/// <summary>A virtual list whose bar keeps a gutter from the rows and has rounded corners.</summary>
public class DemoVirtualList : VirtualList
{
    public DemoVirtualList(string name, int totalItems, float itemHeight)
        : base(totalItems, itemHeight)
    {
        NodeId = name;
        ScrollBox.ScrollBarGutter = 8.0f;
        ScrollBox.VerticalScrollBar.TrackCornerRadius = 8.0f;
        ScrollBox.VerticalScrollBar.ThumbCornerRadius = 8.0f;
    }
}
