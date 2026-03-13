using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides moving average calculations over quote data.
/// </summary>
public static class MovingAverage
{
    /// <summary>
    /// Computes the simple moving average (SMA) of closing prices over the specified period.
    /// </summary>
    /// <param name="quotes">Chronologically ordered quotes.</param>
    /// <param name="period">The number of data points to average.</param>
    /// <returns>A sequence of (Timestamp, Value) pairs for each point where the SMA is defined.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static IReadOnlyList<(DateTimeOffset Timestamp, decimal Value)> Simple(
        IReadOnlyList<Quote> quotes,
        int period)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        if (quotes.Count < period)
            return [];

        var results = new List<(DateTimeOffset, decimal)>(quotes.Count - period + 1);
        var sum = 0m;

        for (var i = 0; i < quotes.Count; i++)
        {
            sum += quotes[i].Close;

            if (i >= period)
                sum -= quotes[i - period].Close;

            if (i >= period - 1)
                results.Add((quotes[i].Timestamp, sum / period));
        }

        return results;
    }
}
