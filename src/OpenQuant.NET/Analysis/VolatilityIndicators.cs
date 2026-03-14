using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides volatility indicator calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages.
/// </summary>
public static class VolatilityIndicators
{
    /// <summary>
    /// Creates a transform block that computes the Average True Range (ATR) using Wilder's
    /// smoothing method over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Atr(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevClose = null;
        var count = 0;
        var sum = 0m;
        var atr = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevClose.HasValue)
                {
                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    count++;

                    if (count <= period)
                    {
                        sum += tr;

                        if (count == period)
                        {
                            atr = sum / period;
                            enriched.Indicators[name] = atr;
                        }
                    }
                    else
                    {
                        atr = ((atr * (period - 1)) + tr) / period;
                        enriched.Indicators[name] = atr;
                    }
                }

                prevClose = c.Close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the Normalized Average True Range (NATR):
    /// <c>(ATR / Close) × 100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Natr(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevClose = null;
        var count = 0;
        var sum = 0m;
        var atr = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevClose.HasValue)
                {
                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    count++;

                    if (count <= period)
                    {
                        sum += tr;

                        if (count == period)
                        {
                            atr = sum / period;
                            enriched.Indicators[name] = c.Close == 0m ? 0m : (atr / c.Close) * 100m;
                        }
                    }
                    else
                    {
                        atr = ((atr * (period - 1)) + tr) / period;
                        enriched.Indicators[name] = c.Close == 0m ? 0m : (atr / c.Close) * 100m;
                    }
                }

                prevClose = c.Close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes Bollinger Bands: a middle band (SMA) and
    /// upper/lower bands at ± <paramref name="nbDev"/> standard deviations.
    /// Outputs: <c>{name}_Upper</c>, <c>{name}_Middle</c>, <c>{name}_Lower</c>.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="period">The SMA period (default 20).</param>
    /// <param name="nbDev">Number of standard deviations (default 2).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> BBands(
        string name,
        int period = 20,
        decimal nbDev = 2m,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var sum = 0m;
        var sumSq = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                sum += close;
                sumSq += close * close;
                window.Enqueue(close);

                if (window.Count > period)
                {
                    var old = window.Dequeue();
                    sum -= old;
                    sumSq -= old * old;
                }

                if (window.Count == period)
                {
                    var n = (decimal)period;
                    var sma = sum / n;
                    var variance = (sumSq / n) - (sma * sma);
                    var stdDev = (decimal)Math.Sqrt((double)variance) * nbDev;

                    enriched.Indicators[name + "_Upper"] = sma + stdDev;
                    enriched.Indicators[name + "_Middle"] = sma;
                    enriched.Indicators[name + "_Lower"] = sma - stdDev;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    // Lookback methods

    /// <summary>Returns the number of input bars consumed before <see cref="Atr"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int AtrLookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="Natr"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int NatrLookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="BBands"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int BBandsLookback(int period = 20) => period - 1;
}
