using System;
using System.Diagnostics;
using System.Globalization;
using System.Threading.Tasks;

using FuiCs;
using FuiCs.Workers;

namespace FuiCsDemo.AdvancedWorker;

/// <summary>The advanced route's worker jobs.</summary>
public static class FuiWorker
{
    public static void Register(WorkerEntries entries) => entries
        .Add("largestPrimeCalculatorWorker", input => new LargestPrimeCalculatorJob().Start(input));

    /// <summary>
    /// Searches for primes for five seconds, reporting the elapsed percentage and yielding every 10 ms; a cancel takes
    /// effect at the next yield and fails the job with <c>cancelled:</c> and the percentage reached.
    /// </summary>
    private sealed class LargestPrimeCalculatorJob : WorkerJob
    {
        private static readonly TimeSpan Total = TimeSpan.FromSeconds(5.0);
        private static readonly TimeSpan YieldInterval = TimeSpan.FromMilliseconds(10.0);
        private const int TimeCheckMask = 127;

        protected override async Task Run()
        {
            var clock = Stopwatch.StartNew();
            var nextYieldAt = YieldInterval;
            var candidate = 2;
            var largestPrime = 2;
            while (true)
            {
                if (IsCancelled)
                {
                    Fail("cancelled:" + Percent(clock.Elapsed).ToString(CultureInfo.InvariantCulture));
                    return;
                }
                var sliceEnd = nextYieldAt < Total ? nextYieldAt : Total;
                var now = clock.Elapsed;
                while (now < sliceEnd)
                {
                    if (IsPrime(candidate))
                    {
                        largestPrime = candidate;
                    }
                    candidate += 1;
                    if ((candidate & TimeCheckMask) == 0)
                    {
                        now = clock.Elapsed;
                    }
                }
                now = clock.Elapsed;
                ReportProgress(Percent(now).ToString(CultureInfo.InvariantCulture));
                if (now >= Total)
                {
                    Complete(largestPrime.ToString(CultureInfo.InvariantCulture));
                    return;
                }
                nextYieldAt += YieldInterval;
                if (nextYieldAt > Total)
                {
                    nextYieldAt = Total;
                }
                await YieldAsync();
            }
        }

        private static int Percent(TimeSpan elapsed) =>
            elapsed <= TimeSpan.Zero ? 0 : elapsed >= Total ? 100 : (int)(elapsed.TotalMilliseconds * 100.0 / Total.TotalMilliseconds);

        private static bool IsPrime(int value)
        {
            if (value < 2)
            {
                return false;
            }
            if (value == 2)
            {
                return true;
            }
            if ((value & 1) == 0)
            {
                return false;
            }
            // One division per step: the remainder is zero exactly when the quotient times the divisor gives the value
            // back, so the quotient serves both the bound and the test.
            var divisor = 3;
            var quotient = value / divisor;
            while (divisor <= quotient)
            {
                if (quotient * divisor == value)
                {
                    return false;
                }
                divisor += 2;
                quotient = value / divisor;
            }
            return true;
        }
    }
}
