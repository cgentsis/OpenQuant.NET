using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides additional volatility indicators as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Results are stored in
/// <see cref="EnrichedCandle.Indicators"/> under the method name by default.
/// </summary>
public static class VolatilityExtra
{
    /// <summary>
    /// Creates a transform block that computes the Average Day Range, the SMA of high minus low.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AverageDayRange(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(AverageDayRange);
        var ranges = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(ranges, candle.High - candle.Low, period);
                if (ranges.Count == period)
                {
                    enriched.Indicators[key] = ranges.Average();
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="AverageDayRange"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int AverageDayRangeLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes Bollinger band width as a percentage of the middle band.
    /// Outputs <c>{name}</c>, <c>{name}_Upper</c>, <c>{name}_Middle</c>, <c>{name}_Lower</c> and <c>{name}_Width</c>.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="nbDev">The number of standard deviations. Defaults to 2.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> BollingerBandWidth(
        int period,
        decimal nbDev = 2m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(BollingerBandWidth);
        var closes = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                Enqueue(closes, enriched.Candle.Close, period);
                if (closes.Count == period)
                {
                    var middle = closes.Average();
                    var deviation = StandardDeviation(closes, middle);
                    var upper = middle + (nbDev * deviation);
                    var lower = middle - (nbDev * deviation);
                    var width = upper - lower;

                    enriched.Indicators[key + "_Upper"] = upper;
                    enriched.Indicators[key + "_Middle"] = middle;
                    enriched.Indicators[key + "_Lower"] = lower;

                    if (middle != 0m)
                    {
                        var percentWidth = (width / middle) * 100m;
                        enriched.Indicators[key] = percentWidth;
                        enriched.Indicators[key + "_Width"] = percentWidth;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="BollingerBandWidth"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int BollingerBandWidthLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes Chaikin volatility, the percentage change between
    /// the current EMA of the high-low range and the EMA recorded <paramref name="period"/> bars earlier.
    /// </summary>
    /// <param name="period">The displacement in EMA values.</param>
    /// <param name="emaPeriod">The EMA period. Defaults to 10.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinVolatility(
        int period,
        int emaPeriod = 10,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(emaPeriod, 1);
        var key = name ?? nameof(ChaikinVolatility);
        var smoothing = 2m / (emaPeriod + 1m);
        var ranges = new Queue<decimal>();
        var emaValues = new Queue<decimal>();
        var ema = 0m;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                var range = candle.High - candle.Low;

                if (!seeded)
                {
                    Enqueue(ranges, range, emaPeriod);
                    if (ranges.Count == emaPeriod)
                    {
                        ema = ranges.Average();
                        seeded = true;
                        Enqueue(emaValues, ema, period + 1);
                    }

                    return enriched;
                }

                ema = Helpers.ComputeEMA(ema, range, smoothing);
                Enqueue(emaValues, ema, period + 1);

                if (emaValues.Count == period + 1)
                {
                    var oldValue = emaValues.Peek();
                    if (oldValue != 0m)
                    {
                        enriched.Indicators[key] = ((ema - oldValue) / oldValue) * 100m;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ChaikinVolatility"/> produces a value.</summary>
    /// <param name="period">The displacement in EMA values.</param>
    /// <param name="emaPeriod">The EMA period.</param>
    /// <returns>The lookback count.</returns>
    public static int ChaikinVolatilityLookback(int period = 14, int emaPeriod = 10) => emaPeriod - 1 + period;

    /// <summary>
    /// Creates a transform block that computes the Mass Index, the sum of the ratio between two
    /// EMAs of the high-low range over the configured period.
    /// </summary>
    /// <param name="period">The summation period. Defaults to 25.</param>
    /// <param name="emaPeriod">The EMA period. Defaults to 9.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MassIndex(
        int period = 25,
        int emaPeriod = 9,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(emaPeriod, 1);
        var key = name ?? nameof(MassIndex);
        var smoothing = 2m / (emaPeriod + 1m);
        var ranges = new Queue<decimal>();
        var ratios = new Queue<decimal>();
        var emaFast = 0m;
        var emaSlow = 0m;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                var range = candle.High - candle.Low;

                if (!seeded)
                {
                    Enqueue(ranges, range, emaPeriod);
                    if (ranges.Count == emaPeriod)
                    {
                        emaFast = ranges.Average();
                        emaSlow = emaFast;
                        seeded = true;
                        Enqueue(ratios, emaFast / emaSlow, period);
                    }

                    return enriched;
                }

                emaFast = Helpers.ComputeEMA(emaFast, range, smoothing);
                emaSlow = Helpers.ComputeEMA(emaSlow, emaFast, smoothing);

                if (emaSlow != 0m)
                {
                    Enqueue(ratios, emaFast / emaSlow, period);
                    if (ratios.Count == period)
                    {
                        enriched.Indicators[key] = ratios.Sum();
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="MassIndex"/> produces a value.</summary>
    /// <param name="period">The summation period.</param>
    /// <param name="emaPeriod">The EMA period.</param>
    /// <returns>The lookback count.</returns>
    public static int MassIndexLookback(int period = 25, int emaPeriod = 9) => emaPeriod - 1 + period - 1;

    /// <summary>
    /// Creates a transform block that computes Percent B, the position of the close price within
    /// the Bollinger bands expressed as a fraction from zero to one. Outputs <c>{name}</c>,
    /// <c>{name}_Upper</c>, <c>{name}_Middle</c> and <c>{name}_Lower</c>.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="nbDev">The number of standard deviations. Defaults to 2.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PercentB(
        int period,
        decimal nbDev = 2m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(PercentB);
        var closes = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                Enqueue(closes, close, period);
                if (closes.Count == period)
                {
                    var middle = closes.Average();
                    var deviation = StandardDeviation(closes, middle);
                    var lower = middle - (nbDev * deviation);
                    var upper = middle + (nbDev * deviation);
                    var bandWidth = upper - lower;

                    enriched.Indicators[key + "_Upper"] = upper;
                    enriched.Indicators[key + "_Middle"] = middle;
                    enriched.Indicators[key + "_Lower"] = lower;

                    if (bandWidth != 0m)
                    {
                        enriched.Indicators[key] = (close - lower) / bandWidth;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="PercentB"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int PercentBLookback(int period = 14) => period - 1;

    private static ExecutionDataflowBlockOptions CreateOptions(CancellationToken cancellationToken) =>
        new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 1,
        };

    private static void Enqueue<T>(Queue<T> buffer, T value, int capacity)
    {
        if (buffer.Count == capacity)
        {
            buffer.Dequeue();
        }

        buffer.Enqueue(value);
    }

    private static decimal StandardDeviation(IEnumerable<decimal> values, decimal mean)
    {
        var sumOfSquares = values.Sum(value => (value - mean) * (value - mean));
        return (decimal)Math.Sqrt((double)(sumOfSquares / values.Count()));
    }
}
