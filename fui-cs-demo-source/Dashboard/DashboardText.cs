using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Dashboard;

/// <summary>Demo text at a fixed size.</summary>
internal sealed class DashboardText : DemoText
{
    public DashboardText(string content = "", float size = 14.0f, DemoTextRecipe recipe = DemoTextRecipe.Body, bool selectable = true)
        : base(content, recipe, selectable)
    {
        FontSize = size;
    }
}
