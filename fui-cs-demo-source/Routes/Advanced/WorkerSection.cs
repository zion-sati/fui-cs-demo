using System;
using System.Globalization;

using FuiCs;
using FuiCsDemo.Shared;

namespace FuiCsDemo.Advanced;

/// <summary>
/// A five-second prime search in the route's worker, with a progress bar, and a cancel that takes effect when the
/// worker next yields.
/// </summary>
internal sealed class WorkerSection : IDisposable
{
    /// <summary>The worker project's published folder, relative to the route's page.</summary>
    private const string WorkerPath = "workers/FuiCsDemo.AdvancedWorker";

    private readonly ProgressBar progressBar = new(0.0f);
    private readonly DemoText statusText = new("", DemoTextStyle.Caption);
    private readonly DemoText detailText = new("", DemoTextStyle.BodySecondary);
    private Worker? active;

    public WorkerSection()
    {
        // In the compact layout the bar fills its panel.
        Application.OnUnmount(Responsive.Subscribe(compact =>
        {
            if (compact)
            {
                progressBar.FillLength();
            }
            else
            {
                progressBar.Length(320.0f);
            }
        }).Dispose);
    }

    public void Dispose()
    {
        active?.Dispose();
        active = null;
    }

    public Node[] Build() =>
    [
        new DemoText("ProgressBar + Worker Sample", DemoTextStyle.Heading3),
        Spacing.Vertical(Spacing.TitleToSupportingGap),
        progressBar,
        Spacing.Vertical(12.0f),
        FlexBox.Row(
                new DemoButton("Start prime worker").Width(170.0f).Height(60.0f).OnClick(_ => Start()),
                Spacing.Horizontal(12.0f),
                new DemoButton("Cancel prime worker").Width(170.0f).Height(60.0f).OnClick(_ => active?.Cancel()))
            .FillWidth()
            .FlexWrap(FlexWrap.Wrap)
            .AlignItems(AlignItems.Stretch),
        Spacing.Vertical(10.0f),
        statusText,
        Spacing.Vertical(6.0f),
        detailText,
        Spacing.Vertical(10.0f),
        new DemoText(
            "This sample connects the Worker API to a retained ProgressBar. Start runs a 5-second prime search with frequent cooperative yields; cancel waits for the next yield and then reports cancellation.",
            DemoTextStyle.BodySecondary),
    ];

    private void Start()
    {
        if (active is not null)
        {
            return;
        }
        progressBar.Value = 0.0f;
        statusText.Text = "Worker: computing primes...";
        detailText.Text = "";
        active = new Worker(WorkerPath, "largestPrimeCalculatorWorker")
            .OnProgress(e =>
            {
                var percent = LeadingPercent(e.Message);
                progressBar.Value = percent;
                detailText.Text = "Prime search progress: " + percent.ToString(CultureInfo.InvariantCulture) + "%.";
            })
            .OnCompleted(e =>
            {
                active = null;
                progressBar.Value = 100.0f;
                statusText.Text = "Worker: completed";
                detailText.Text = "Largest prime after 5s: " + e.Result;
            })
            .OnError(e =>
            {
                active = null;
                if (e.Message.StartsWith("cancelled:", StringComparison.Ordinal))
                {
                    var percent = LeadingPercent(e.Message);
                    progressBar.Value = percent;
                    statusText.Text = "Worker: cancelled";
                    detailText.Text = "Prime search cancelled after yielding at " + percent.ToString(CultureInfo.InvariantCulture) + "%.";
                }
                else
                {
                    statusText.Text = "Worker: error";
                    detailText.Text = "Worker error: " + e.Message;
                }
            });
        active.Start("advanced-demo");
    }

    /// <summary>The first run of digits in <paramref name="text"/>, or 0.</summary>
    private static int LeadingPercent(string text)
    {
        var value = 0;
        var sawDigit = false;
        foreach (var character in text)
        {
            if (char.IsAsciiDigit(character))
            {
                sawDigit = true;
                value = (value * 10) + (character - '0');
            }
            else if (sawDigit)
            {
                break;
            }
        }
        return value;
    }
}
