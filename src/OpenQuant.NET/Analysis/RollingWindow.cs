using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides rolling-window primitive calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. These indicators operate on a sliding
/// window of recent values and produce output once the window is full.
/// </summary>
public static class RollingWindow
{
    /// <summary>
    /// Creates a transform block that computes the rolling sum of closing prices over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sum(
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
                    enriched.Indicators[name] = sum;
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
    /// Creates a transform block that computes the rolling maximum of closing prices over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Max(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    enriched.Indicators[name] = window.Max();
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
    /// Creates a transform block that computes the rolling minimum of closing prices over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Min(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    enriched.Indicators[name] = window.Min();
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
    /// Creates a transform block that computes the index of the highest closing price within the
    /// rolling window. The index is 0-based from the oldest element in the window (0 = oldest,
    /// period − 1 = most recent). For ties, the most recent occurrence wins.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MaxIndex(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    var maxVal = decimal.MinValue;
                    var maxIdx = 0;
                    var idx = 0;

                    foreach (var val in window)
                    {
                        if (val >= maxVal)
                        {
                            maxVal = val;
                            maxIdx = idx;
                        }

                        idx++;
                    }

                    enriched.Indicators[name] = maxIdx;
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
    /// Creates a transform block that computes the index of the lowest closing price within the
    /// rolling window. The index is 0-based from the oldest element (0 = oldest, period − 1 = most recent).
    /// For ties, the most recent occurrence wins.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MinIndex(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    var minVal = decimal.MaxValue;
                    var minIdx = 0;
                    var idx = 0;

                    foreach (var val in window)
                    {
                        if (val <= minVal)
                        {
                            minVal = val;
                            minIdx = idx;
                        }

                        idx++;
                    }

                    enriched.Indicators[name] = minIdx;
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
    /// Creates a transform block that computes both the rolling minimum and maximum of closing
    /// prices over the specified period. Values are stored under <c>{name}_Min</c> and <c>{name}_Max</c>.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MinMax(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    enriched.Indicators[name + "_Min"] = window.Min();
                    enriched.Indicators[name + "_Max"] = window.Max();
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
    /// Creates a transform block that computes the indexes of both the lowest and highest closing
    /// prices in the rolling window. Values are stored under <c>{name}_MinIdx</c> and <c>{name}_MaxIdx</c>.
    /// Indexes are 0-based from oldest (0 = oldest, period − 1 = most recent).
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MinMaxIndex(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    var minVal = decimal.MaxValue;
                    var maxVal = decimal.MinValue;
                    var minIdx = 0;
                    var maxIdx = 0;
                    var idx = 0;

                    foreach (var val in window)
                    {
                        if (val <= minVal)
                        {
                            minVal = val;
                            minIdx = idx;
                        }

                        if (val >= maxVal)
                        {
                            maxVal = val;
                            maxIdx = idx;
                        }

                        idx++;
                    }

                    enriched.Indicators[name + "_MinIdx"] = minIdx;
                    enriched.Indicators[name + "_MaxIdx"] = maxIdx;
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
    /// Creates a transform block that computes the midpoint of closing prices over the specified
    /// period: (highest close + lowest close) / 2.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MidPoint(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);

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
                    enriched.Indicators[name] = (window.Max() + window.Min()) / 2m;
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
    /// Creates a transform block that computes the midpoint price over the specified period:
    /// (highest high + lowest low) / 2.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MidPrice(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var highs = new Queue<decimal>(period);
        var lows = new Queue<decimal>(period);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                highs.Enqueue(enriched.Candle.High);
                lows.Enqueue(enriched.Candle.Low);

                if (highs.Count > period)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == period)
                {
                    enriched.Indicators[name] = (highs.Max() + lows.Min()) / 2m;
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
    /// Creates a transform block that computes momentum: Close − Close[n periods ago].
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Mom(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period + 1)
                {
                    window.Dequeue();
                }

                if (window.Count == period + 1)
                {
                    enriched.Indicators[name] = enriched.Candle.Close - window.Peek();
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
    /// Creates a transform block that computes the rate of change:
    /// ((Close / Close[n periods ago]) − 1) × 100.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Roc(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period + 1)
                {
                    window.Dequeue();
                }

                if (window.Count == period + 1)
                {
                    var prev = window.Peek();

                    if (prev != 0m)
                    {
                        enriched.Indicators[name] = ((enriched.Candle.Close / prev) - 1m) * 100m;
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

    /// <summary>
    /// Creates a transform block that computes the rate of change percentage:
    /// (Close − Close[n]) / Close[n].
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Rocp(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period + 1)
                {
                    window.Dequeue();
                }

                if (window.Count == period + 1)
                {
                    var prev = window.Peek();

                    if (prev != 0m)
                    {
                        enriched.Indicators[name] = (enriched.Candle.Close - prev) / prev;
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

    /// <summary>
    /// Creates a transform block that computes the rate of change ratio: Close / Close[n periods ago].
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Rocr(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period + 1)
                {
                    window.Dequeue();
                }

                if (window.Count == period + 1)
                {
                    var prev = window.Peek();

                    if (prev != 0m)
                    {
                        enriched.Indicators[name] = enriched.Candle.Close / prev;
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

    /// <summary>
    /// Creates a transform block that computes the rate of change ratio scaled to 100:
    /// (Close / Close[n periods ago]) × 100.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Rocr100(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period + 1)
                {
                    window.Dequeue();
                }

                if (window.Count == period + 1)
                {
                    var prev = window.Peek();

                    if (prev != 0m)
                    {
                        enriched.Indicators[name] = (enriched.Candle.Close / prev) * 100m;
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
}
