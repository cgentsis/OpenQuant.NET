using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides moving average calculations as TPL Dataflow <see cref="TransformBlock{TInput,TOutput}"/> stages.
/// Each factory returns a block that enriches an <see cref="EnrichedCandle"/> with the computed indicator
/// value and passes it downstream. Blocks can be chained via
/// <see cref="DataflowBlock.LinkTo{TOutput}(ISourceBlock{TOutput},ITargetBlock{TOutput},DataflowLinkOptions)"/>
/// with <c>PropagateCompletion = true</c>.
/// </summary>
public static class MovingAverage
{
    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// simple moving average (SMA) of closing prices over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points to average.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> SMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var sum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                sum += enriched.Candle.Close;
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period)
                {
                    sum -= window.Dequeue();
                }

                if (window.Count == period)
                {
                    enriched.Indicators[name] = sum / period;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// exponential moving average (EMA) of closing prices over the specified period.
    /// The first value emitted is the SMA of the initial <paramref name="period"/> data points;
    /// subsequent values use the smoothing factor <c>k = 2 / (period + 1)</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points used for the initial SMA and smoothing factor.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> EMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var count = 0;
        var sum = 0m;
        var ema = 0m;
        var k = 2m / (period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                count++;

                if (count <= period)
                {
                    sum += enriched.Candle.Close;

                    if (count == period)
                    {
                        ema = sum / period;
                        enriched.Indicators[name] = ema;
                    }
                }
                else
                {
                    ema = (enriched.Candle.Close * k) + (ema * (1 - k));
                    enriched.Indicators[name] = ema;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// weighted moving average (WMA) of closing prices over the specified period.
    /// Weights increase linearly: the oldest value in the window receives weight 1, the newest
    /// receives weight equal to <paramref name="period"/>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> WMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var divisor = period * (period + 1) / 2m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period)
                {
                    window.Dequeue();
                }

                if (window.Count == period)
                {
                    var weightedSum = 0m;
                    var weight = 1;

                    foreach (var price in window)
                    {
                        weightedSum += price * weight;
                        weight++;
                    }

                    enriched.Indicators[name] = weightedSum / divisor;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// Hull moving average (HMA) of closing prices over the specified period.
    /// HMA is calculated as <c>WMA(√n)</c> of <c>2 × WMA(n/2) − WMA(n)</c>, which reduces lag
    /// while maintaining smoothness.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points for the full WMA window. Must be at least 2.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var halfPeriod = period / 2;
        var sqrtPeriod = Math.Max(1, (int)Math.Round(Math.Sqrt(period)));

        var priceBuffer = new decimal[period];
        var priceIndex = 0;
        var priceCount = 0;

        var diffBuffer = new decimal[sqrtPeriod];
        var diffIndex = 0;
        var diffCount = 0;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                priceBuffer[priceIndex] = enriched.Candle.Close;
                priceIndex = (priceIndex + 1) % period;
                priceCount = Math.Min(priceCount + 1, period);

                if (priceCount == period)
                {
                    var wmaFull = ComputeWMACircular(priceBuffer, priceIndex, period);
                    var wmaHalf = ComputeWMACircular(priceBuffer, (priceIndex + period - halfPeriod) % period, halfPeriod);
                    var diff = (2m * wmaHalf) - wmaFull;

                    diffBuffer[diffIndex] = diff;
                    diffIndex = (diffIndex + 1) % sqrtPeriod;
                    diffCount = Math.Min(diffCount + 1, sqrtPeriod);

                    if (diffCount == sqrtPeriod)
                    {
                        var hma = ComputeWMACircular(diffBuffer, diffIndex, sqrtPeriod);
                        enriched.Indicators[name] = hma;
                    }
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    private static decimal ComputeWMACircular(decimal[] buffer, int startIndex, int count)
    {
        var weightedSum = 0m;
        var length = buffer.Length;

        for (var i = 0; i < count; i++)
        {
            weightedSum += buffer[(startIndex + i) % length] * (i + 1);
        }

        return weightedSum / (count * (count + 1) / 2m);
    }
}
