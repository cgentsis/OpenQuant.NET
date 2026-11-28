using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides additional volume-based indicators as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Results are stored in
/// <see cref="EnrichedCandle.Indicators"/> under the method name by default.
/// </summary>
public static class VolumeExtra
{
    /// <summary>
    /// Creates a transform block that computes Chaikin Money Flow, the volume-weighted average of
    /// the close location value within the high-low range.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinMoneyFlow(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(ChaikinMoneyFlow);
        var flows = new Queue<decimal>();
        var volumes = new Queue<long>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                var range = candle.High - candle.Low;
                var multiplier = range == 0m ? 0m : (((candle.Close - candle.Low) - (candle.High - candle.Close)) / range);
                Enqueue(flows, multiplier * candle.Volume, period);
                Enqueue(volumes, candle.Volume, period);

                if (flows.Count == period)
                {
                    var totalVolume = volumes.Sum();
                    if (totalVolume != 0L)
                    {
                        enriched.Indicators[key] = flows.Sum() / totalVolume;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ChaikinMoneyFlow"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int ChaikinMoneyFlowLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the Elder Force Index, an EMA of price change
    /// multiplied by volume. The first value is produced on the second bar.
    /// </summary>
    /// <param name="period">The EMA period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ElderForceIndex(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(ElderForceIndex);
        var smoothing = 2m / (period + 1m);
        var previousClose = 0m;
        var ema = 0m;
        var seeded = false;
        var first = true;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (first)
                {
                    first = false;
                }
                else
                {
                    var force = (candle.Close - previousClose) * candle.Volume;
                    ema = seeded
                        ? Helpers.ComputeEMA(ema, force, smoothing)
                        : force;
                    seeded = true;
                    enriched.Indicators[key] = ema;
                }

                previousClose = candle.Close;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ElderForceIndex"/> produces a value.</summary>
    /// <param name="period">The EMA period, ignored because the first change seeds the EMA.</param>
    /// <returns>The lookback count.</returns>
    public static int ElderForceIndexLookback(int period = 14) => 1;

    /// <summary>
    /// Creates a transform block that computes Arms Ease of Movement: the distance between
    /// successive midpoints divided by the volume-to-range box ratio.
    /// </summary>
    /// <param name="period">The SMA period.</param>
    /// <param name="volumeDivisor">The divisor used to scale volume. Defaults to 10000.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is less than or equal to zero.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ArmsEaseOfMovement(
        int period,
        decimal volumeDivisor = 10000m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(volumeDivisor, 0m);
        var key = name ?? nameof(ArmsEaseOfMovement);
        var values = new Queue<decimal>();
        decimal? previousMid = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (previousMid.HasValue)
                {
                    var range = candle.High - candle.Low;
                    if (range != 0m && candle.Volume != 0L)
                    {
                        var mid = (candle.High + candle.Low) / 2m;
                        var boxRatio = (candle.Volume / volumeDivisor) / range;
                        Enqueue(values, (mid - previousMid.Value) / boxRatio, period);

                        if (values.Count == period)
                        {
                            enriched.Indicators[key] = values.Average();
                        }
                    }
                }

                previousMid = (candle.High + candle.Low) / 2m;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ArmsEaseOfMovement"/> produces a value.</summary>
    /// <param name="period">The SMA period.</param>
    /// <returns>The lookback count.</returns>
    public static int ArmsEaseOfMovementLookback(int period = 14) => period;

    /// <summary>
    /// Creates a transform block that computes the Klinger Volume Oscillator, the difference
    /// between fast and slow EMAs of signed volume.
    /// </summary>
    /// <param name="period">The fast EMA period.</param>
    /// <param name="slowPeriod">The slow EMA period. Defaults to 34.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KlingerVolumeOscillator(
        int period,
        int slowPeriod = 34,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);
        var key = name ?? nameof(KlingerVolumeOscillator);
        var fastSmoothing = 2m / (period + 1m);
        var slowSmoothing = 2m / (slowPeriod + 1m);
        var fastSeed = new Queue<decimal>();
        var slowSeed = new Queue<decimal>();
        var fastEma = 0m;
        var slowEma = 0m;
        var fastSeeded = false;
        var slowSeeded = false;
        var previousClose = 0m;
        var first = true;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (first)
                {
                    first = false;
                }
                else
                {
                    decimal sign;
                    if (candle.Close > previousClose)
                    {
                        sign = 1m;
                    }
                    else if (candle.Close < previousClose)
                    {
                        sign = -1m;
                    }
                    else
                    {
                        sign = 0m;
                    }

                    var signedVolume = sign * candle.Volume;
                    Enqueue(fastSeed, signedVolume, period);
                    Enqueue(slowSeed, signedVolume, slowPeriod);

                    if (!fastSeeded && fastSeed.Count == period)
                    {
                        fastEma = fastSeed.Average();
                        fastSeeded = true;
                    }
                    else if (fastSeeded)
                    {
                        fastEma = Helpers.ComputeEMA(fastEma, signedVolume, fastSmoothing);
                    }

                    if (!slowSeeded && slowSeed.Count == slowPeriod)
                    {
                        slowEma = slowSeed.Average();
                        slowSeeded = true;
                    }
                    else if (slowSeeded)
                    {
                        slowEma = Helpers.ComputeEMA(slowEma, signedVolume, slowSmoothing);
                    }

                    if (fastSeeded && slowSeeded)
                    {
                        enriched.Indicators[key] = fastEma - slowEma;
                    }
                }

                previousClose = candle.Close;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="KlingerVolumeOscillator"/> produces a value.</summary>
    /// <param name="period">The fast EMA period.</param>
    /// <param name="slowPeriod">The slow EMA period.</param>
    /// <returns>The lookback count.</returns>
    public static int KlingerVolumeOscillatorLookback(int period = 14, int slowPeriod = 34) => slowPeriod;

    /// <summary>
    /// Creates a transform block that computes the Market Facilitation Index, the high-low range
    /// per unit of volume scaled by <paramref name="scale"/>. The period is accepted for API
    /// consistency but does not affect the stateless calculation.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="scale">The scale factor. Defaults to 100000.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a parameter is not positive.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MarketFacilitationIndex(
        int period,
        decimal scale = 100000m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(scale, 0m);
        var key = name ?? nameof(MarketFacilitationIndex);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (candle.Volume != 0L)
                {
                    enriched.Indicators[key] = ((candle.High - candle.Low) / candle.Volume) * scale;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="MarketFacilitationIndex"/> produces a value.</summary>
    /// <param name="period">The period, ignored because the indicator is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int MarketFacilitationIndexLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Positive Volume Index. It starts at 100 and
    /// increases by the closing-price ratio only on bars with higher volume than the previous bar.
    /// The period is accepted for API consistency but does not affect the calculation.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeIndex(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(PositiveVolumeIndex);
        var pvi = 100m;
        var previousClose = 0m;
        var previousVolume = 0L;
        var first = true;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (first)
                {
                    first = false;
                }
                else if (candle.Volume > previousVolume && previousClose != 0m)
                {
                    pvi *= 1m + ((candle.Close - previousClose) / previousClose);
                }

                enriched.Indicators[key] = pvi;
                previousClose = candle.Close;
                previousVolume = candle.Volume;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="PositiveVolumeIndex"/> produces a value.</summary>
    /// <param name="period">The period, ignored because the first bar seeds the index.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int PositiveVolumeIndexLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Positive Volume Oscillator, the percentage
    /// change of the Positive Volume Index over the configured period.
    /// </summary>
    /// <param name="period">The lookback period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeOscillator(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(PositiveVolumeOscillator);
        var pvi = 100m;
        var previousClose = 0m;
        var previousVolume = 0L;
        var first = true;
        var pviHistory = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (first)
                {
                    first = false;
                }
                else if (candle.Volume > previousVolume && previousClose != 0m)
                {
                    pvi *= 1m + ((candle.Close - previousClose) / previousClose);
                }

                previousClose = candle.Close;
                previousVolume = candle.Volume;
                Enqueue(pviHistory, pvi, period + 1);

                if (pviHistory.Count == period + 1)
                {
                    var pastPvi = pviHistory.Peek();
                    if (pastPvi != 0m)
                    {
                        enriched.Indicators[key] = ((pvi / pastPvi) - 1m) * 100m;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="PositiveVolumeOscillator"/> produces a value.</summary>
    /// <param name="period">The lookback period.</param>
    /// <returns>The lookback count.</returns>
    public static int PositiveVolumeOscillatorLookback(int period = 14) => period;

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
}
