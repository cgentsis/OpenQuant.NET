using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides additional overlapping studies as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Results are stored in
/// <see cref="EnrichedCandle.Indicators"/> using the method name by default, and multi-value
/// indicators use suffixed keys such as <c>_Upper</c>, <c>_Middle</c> and <c>_Lower</c>.
/// </summary>
public static class OverlapStudiesExtra
{
    /// <summary>
    /// Creates a transform block that computes smoothed typical price and upper/lower bands
    /// derived from the high-low range. Outputs <c>{name}_Middle</c>, <c>{name}_Upper</c> and <c>{name}_Lower</c>.
    /// </summary>
    /// <param name="period">The smoothing period.</param>
    /// <param name="upperFactor">The multiplier applied to the relative range. Defaults to 4.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AccelerationBands(
        int period,
        decimal upperFactor = 4m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var prefix = name ?? nameof(AccelerationBands);
        var typicalBuffer = new Queue<decimal>();
        var upperBuffer = new Queue<decimal>();
        var lowerBuffer = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                var rangeDenominator = candle.High + candle.Low;
                if (rangeDenominator != 0m)
                {
                    var typical = (candle.High + candle.Low + candle.Close) / 3m;
                    var relativeRange = upperFactor * ((candle.High - candle.Low) / rangeDenominator);
                    Enqueue(typicalBuffer, typical, period);
                    Enqueue(upperBuffer, typical * (1m + relativeRange), period);
                    Enqueue(lowerBuffer, typical * (1m - relativeRange), period);

                    if (typicalBuffer.Count == period)
                    {
                        enriched.Indicators[prefix + "_Middle"] = typicalBuffer.Average();
                        enriched.Indicators[prefix + "_Upper"] = upperBuffer.Average();
                        enriched.Indicators[prefix + "_Lower"] = lowerBuffer.Average();
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="AccelerationBands"/> produces values.</summary>
    /// <param name="period">The smoothing period.</param>
    /// <returns>The lookback count.</returns>
    public static int AccelerationBandsLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the Arnaud Legoux moving average, a Gaussian-weighted
    /// average of closing prices over the period. Outputs a single value under <c>name</c>.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="sigma">The Gaussian width. Defaults to 0.85.</param>
    /// <param name="offset">The weight centre from 0 (recent) to 1 (oldest). Defaults to 0.5.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1 or sigma is not positive.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ArnaudLegouxMA(
        int period,
        decimal sigma = 0.85m,
        decimal offset = 0.5m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(sigma, 0m);
        var key = name ?? nameof(ArnaudLegouxMA);
        var buffer = new Queue<decimal>();
        var centre = offset * (period - 1);
        var denominator = 2m * sigma * sigma;
        var weights = Enumerable.Range(0, period)
            .Select(index => (decimal)Math.Exp(-Math.Pow(index - (double)centre, 2) / (double)denominator))
            .ToArray();
        var weightSum = weights.Sum();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                Enqueue(buffer, enriched.Candle.Close, period);
                if (buffer.Count == period)
                {
                    var weightedSum = 0m;
                    var index = 0;
                    foreach (var value in buffer)
                    {
                        weightedSum += value * weights[index];
                        index++;
                    }

                    enriched.Indicators[key] = weightedSum / weightSum;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ArnaudLegouxMA"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int ArnaudLegouxMALookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the Chande Kroll stop, the highest high minus a
    /// multiple of ATR and the lowest low plus that multiple. Outputs <c>{name}_Short</c> and <c>{name}_Long</c>.
    /// </summary>
    /// <param name="period">The lookback for extreme prices.</param>
    /// <param name="rangePeriod">The ATR period. Defaults to 10.</param>
    /// <param name="factor">The ATR multiplier. Defaults to 1.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChandeKrollStop(
        int period,
        int rangePeriod = 10,
        decimal factor = 1m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rangePeriod, 1);
        var prefix = name ?? nameof(ChandeKrollStop);
        var highs = new Queue<decimal>();
        var lows = new Queue<decimal>();
        var trueRanges = new Queue<decimal>();
        decimal? previousClose = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                if (previousClose.HasValue)
                {
                    Enqueue(trueRanges, TrueRange(candle.High, candle.Low, previousClose.Value), rangePeriod);
                }

                previousClose = candle.Close;
                Enqueue(highs, candle.High, period);
                Enqueue(lows, candle.Low, period);

                if (highs.Count == period && trueRanges.Count == rangePeriod)
                {
                    var averageTrueRange = trueRanges.Average();
                    enriched.Indicators[prefix + "_Short"] = highs.Max() - (factor * averageTrueRange);
                    enriched.Indicators[prefix + "_Long"] = lows.Min() + (factor * averageTrueRange);
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ChandeKrollStop"/> produces values.</summary>
    /// <param name="period">The lookback for extreme prices.</param>
    /// <param name="rangePeriod">The ATR period.</param>
    /// <returns>The lookback count.</returns>
    public static int ChandeKrollStopLookback(int period = 14, int rangePeriod = 10) =>
        Math.Max(period, rangePeriod + 1) - 1;

    /// <summary>
    /// Creates a transform block that computes Donchian channels: the highest high, lowest low and
    /// their midpoint over the period. Outputs <c>{name}_Upper</c>, <c>{name}_Lower</c> and <c>{name}_Middle</c>.
    /// </summary>
    /// <param name="period">The channel period.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DonchianChannels(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var prefix = name ?? nameof(DonchianChannels);
        var highs = new Queue<decimal>();
        var lows = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(highs, candle.High, period);
                Enqueue(lows, candle.Low, period);

                if (highs.Count == period)
                {
                    var upper = highs.Max();
                    var lower = lows.Min();
                    enriched.Indicators[prefix + "_Upper"] = upper;
                    enriched.Indicators[prefix + "_Lower"] = lower;
                    enriched.Indicators[prefix + "_Middle"] = (upper + lower) / 2m;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="DonchianChannels"/> produces values.</summary>
    /// <param name="period">The channel period.</param>
    /// <returns>The lookback count.</returns>
    public static int DonchianChannelsLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the fractal adaptive moving average. Two adjacent
    /// halves of a 2N-bar window determine a fractal dimension which controls an adaptive EMA.
    /// </summary>
    /// <param name="period">The base period; N is max(2, period / 2).</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> FractalAdaptiveMA(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(FractalAdaptiveMA);
        var halfWindow = Math.Max(2, period / 2);
        var windowSize = 2 * halfWindow;
        var highs = new Queue<decimal>();
        var lows = new Queue<decimal>();
        var previous = 0m;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(highs, candle.High, windowSize);
                Enqueue(lows, candle.Low, windowSize);

                if (highs.Count < windowSize)
                {
                    return enriched;
                }

                var highList = highs.ToArray();
                var lowList = lows.ToArray();
                var firstHigh = highList.Take(halfWindow).Max();
                var firstLow = lowList.Take(halfWindow).Min();
                var secondHigh = highList.Skip(halfWindow).Max();
                var secondLow = lowList.Skip(halfWindow).Min();
                var totalHigh = highList.Max();
                var totalLow = lowList.Min();

                var firstRange = (firstHigh - firstLow) / halfWindow;
                var secondRange = (secondHigh - secondLow) / halfWindow;
                var totalRange = (totalHigh - totalLow) / (decimal)windowSize;
                var alpha = 1m;
                if (firstRange + secondRange > 0m && totalRange > 0m)
                {
                    var dimension =
                        ((decimal)Math.Log10((double)(firstRange + secondRange)) -
                         (decimal)Math.Log10((double)totalRange)) /
                        (decimal)Math.Log10(2d);
                    alpha = Math.Clamp((decimal)Math.Exp(-4.6d * ((double)dimension - 1d)), 0.01m, 1m);
                }

                if (!seeded)
                {
                    previous = candle.Close;
                    seeded = true;
                }
                else
                {
                    previous = (alpha * candle.Close) + ((1m - alpha) * previous);
                }

                enriched.Indicators[key] = previous;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="FractalAdaptiveMA"/> produces a value.</summary>
    /// <param name="period">The base period.</param>
    /// <returns>The lookback count.</returns>
    public static int FractalAdaptiveMALookback(int period = 14) => (2 * Math.Max(2, period / 2)) - 1;

    /// <summary>
    /// Creates a transform block that computes Ichimoku Kinko Hyo lines. Outputs <c>{name}_Tenkan</c>,
    /// <c>{name}_Kijun</c>, <c>{name}_SpanA</c> and <c>{name}_SpanB</c>. Spans are displaced forward by
    /// the base period.
    /// </summary>
    /// <param name="period">The base period used to derive defaults.</param>
    /// <param name="conversionPeriod">The tenkan period, or 0 to use max(1, period / 2).</param>
    /// <param name="basePeriod">The kijun and displacement period, or 0 to use period.</param>
    /// <param name="spanBPeriod">The span B period, or 0 to use 2 * period.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> IchimokuKinkoHyo(
        int period,
        int conversionPeriod = 0,
        int basePeriod = 0,
        int spanBPeriod = 0,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var tenkanPeriod = conversionPeriod == 0 ? Math.Max(1, period / 2) : conversionPeriod;
        var kijunPeriod = basePeriod == 0 ? period : basePeriod;
        var spanPeriod = spanBPeriod == 0 ? 2 * period : spanBPeriod;
        ArgumentOutOfRangeException.ThrowIfLessThan(tenkanPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(kijunPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(spanPeriod, 1);

        var prefix = name ?? nameof(IchimokuKinkoHyo);
        var highs = new Queue<decimal>();
        var lows = new Queue<decimal>();
        var spanAValues = new Queue<decimal>();
        var spanBValues = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(highs, candle.High, spanPeriod);
                Enqueue(lows, candle.Low, spanPeriod);

                decimal? tenkan = null;
                if (highs.Count >= tenkanPeriod)
                {
                    tenkan = (highs.TakeLast(tenkanPeriod).Max() + lows.TakeLast(tenkanPeriod).Min()) / 2m;
                    enriched.Indicators[prefix + "_Tenkan"] = tenkan.Value;
                }

                decimal? kijun = null;
                if (highs.Count >= kijunPeriod)
                {
                    kijun = (highs.TakeLast(kijunPeriod).Max() + lows.TakeLast(kijunPeriod).Min()) / 2m;
                    enriched.Indicators[prefix + "_Kijun"] = kijun.Value;
                }

                if (tenkan.HasValue && kijun.HasValue)
                {
                    spanAValues.Enqueue((tenkan.Value + kijun.Value) / 2m);
                }

                if (highs.Count == spanPeriod)
                {
                    spanBValues.Enqueue((highs.Max() + lows.Min()) / 2m);
                }

                if (spanAValues.Count > kijunPeriod)
                {
                    enriched.Indicators[prefix + "_SpanA"] = spanAValues.Dequeue();
                }

                if (spanBValues.Count > kijunPeriod)
                {
                    enriched.Indicators[prefix + "_SpanB"] = spanBValues.Dequeue();
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="IchimokuKinkoHyo"/> produces spans.</summary>
    /// <param name="period">The base period used to derive defaults.</param>
    /// <param name="conversionPeriod">The tenkan period, or 0 for the default.</param>
    /// <param name="basePeriod">The kijun and displacement period, or 0 for the default.</param>
    /// <param name="spanBPeriod">The span B period, or 0 for the default.</param>
    /// <returns>The lookback count.</returns>
    public static int IchimokuKinkoHyoLookback(
        int period = 14,
        int conversionPeriod = 0,
        int basePeriod = 0,
        int spanBPeriod = 0)
    {
        var kijunPeriod = basePeriod == 0 ? period : basePeriod;
        var spanPeriod = spanBPeriod == 0 ? 2 * period : spanBPeriod;
        return spanPeriod - 1 + kijunPeriod;
    }

    /// <summary>
    /// Creates a transform block that computes Keltner channels: a close-price SMA with upper and
    /// lower bands one ATR multiple away. Outputs <c>{name}_Upper</c>, <c>{name}_Middle</c> and <c>{name}_Lower</c>.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="multiplier">The ATR multiplier. Defaults to 2.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KeltnerChannels(
        int period,
        decimal multiplier = 2m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var prefix = name ?? nameof(KeltnerChannels);
        var closes = new Queue<decimal>();
        var trueRanges = new Queue<decimal>();
        decimal? previousClose = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(closes, candle.Close, period);

                if (previousClose.HasValue)
                {
                    Enqueue(trueRanges, TrueRange(candle.High, candle.Low, previousClose.Value), period);
                }

                previousClose = candle.Close;

                if (closes.Count == period && trueRanges.Count == period)
                {
                    var middle = closes.Average();
                    var band = multiplier * trueRanges.Average();
                    enriched.Indicators[prefix + "_Upper"] = middle + band;
                    enriched.Indicators[prefix + "_Middle"] = middle;
                    enriched.Indicators[prefix + "_Lower"] = middle - band;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="KeltnerChannels"/> produces values.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int KeltnerChannelsLookback(int period = 14) => period;

    /// <summary>
    /// Creates a transform block that computes a simplified adaptive moving average whose smoothing
    /// constant adapts to the ratio of an EMA of price changes to an EMA of price.
    /// </summary>
    /// <param name="period">The EMA period used for adaptation.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MESAAdaptiveMA(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(MESAAdaptiveMA);
        var smoothing = 2m / (period + 1m);
        var smoothedDelta = 0m;
        var smoothClose = 0m;
        var previousClose = 0m;
        var previousMama = 0m;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                if (!seeded)
                {
                    smoothClose = close;
                    previousMama = close;
                    seeded = true;
                }
                else
                {
                    var delta = close - previousClose;
                    smoothedDelta = Helpers.ComputeEMA(smoothedDelta, delta, smoothing);
                    smoothClose = Helpers.ComputeEMA(smoothClose, close, smoothing);
                    var alpha = Math.Clamp(
                        Math.Abs(smoothedDelta) / (Math.Abs(smoothClose) + 0.0000001m),
                        0.01m,
                        0.5m);
                    previousMama = (alpha * close) + ((1m - alpha) * previousMama);
                }

                previousClose = close;
                enriched.Indicators[key] = previousMama;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="MESAAdaptiveMA"/> produces a value.</summary>
    /// <param name="period">The EMA period, ignored because the first bar seeds the average.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int MESAAdaptiveMALookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the McGinley dynamic average, an adaptive average
    /// that adjusts its speed using the ratio between price and the previous average.
    /// </summary>
    /// <param name="period">The adjustment period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> McGinleyDynamic(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(McGinleyDynamic);
        var previous = 0m;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                if (!seeded)
                {
                    previous = close;
                    seeded = true;
                }
                else if (previous != 0m && close > 0m)
                {
                    var ratio = (double)(close / previous);
                    previous += (close - previous) / (period * (decimal)Math.Pow(ratio, 4d));
                }

                enriched.Indicators[key] = previous;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="McGinleyDynamic"/> produces a value.</summary>
    /// <param name="period">The adjustment period, ignored because the first bar seeds the average.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int McGinleyDynamicLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the volume-weighted moving average of closing prices.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Vwma(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(Vwma);
        var weighted = new Queue<decimal>();
        var volumes = new Queue<long>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(weighted, candle.Close * candle.Volume, period);
                Enqueue(volumes, candle.Volume, period);

                if (weighted.Count == period)
                {
                    var totalVolume = volumes.Sum();
                    if (totalVolume != 0L)
                    {
                        enriched.Indicators[key] = weighted.Sum() / totalVolume;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Vwma"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    public static int VwmaLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes a simplified streaming zigzag. Outputs
    /// <c>{name}_Zigzag</c> with the current pivot price and <c>{name}_Direction</c> with +1 or -1.
    /// </summary>
    /// <param name="period">The minimum number of bars between pivots.</param>
    /// <param name="reversalPercent">The reversal threshold in percent. Defaults to 5.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Zigzag(
        int period,
        decimal reversalPercent = 5m,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(reversalPercent, 0m);
        var prefix = name ?? nameof(Zigzag);
        var direction = 1;
        var extreme = 0m;
        var barsSincePivot = 0;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                if (!seeded)
                {
                    extreme = close;
                    seeded = true;
                }
                else
                {
                    var reversal = 0m;
                    if (extreme != 0m)
                    {
                        reversal = direction == 1
                            ? ((extreme - close) / extreme) * 100m
                            : ((close - extreme) / extreme) * 100m;
                    }

                    if (barsSincePivot >= period && reversal >= reversalPercent)
                    {
                        direction = -direction;
                        extreme = close;
                        barsSincePivot = 0;
                    }
                    else
                    {
                        if ((direction == 1 && close > extreme) || (direction == -1 && close < extreme))
                        {
                            extreme = close;
                        }

                        barsSincePivot++;
                    }
                }

                enriched.Indicators[prefix + "_Zigzag"] = extreme;
                enriched.Indicators[prefix + "_Direction"] = direction;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Zigzag"/> produces a value.</summary>
    /// <param name="period">The minimum bars between pivots, ignored because the first bar seeds the result.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int ZigzagLookback(int period = 14) => 0;
#pragma warning restore S3400

#pragma warning disable S1133
    /// <summary>
    /// Creates a transform block that computes the volume-weighted moving average.
    /// </summary>
    /// <param name="period">The averaging period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    [Obsolete("Use Vwma instead.")]
    public static TransformBlock<EnrichedCandle, EnrichedCandle> VWMA(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        Vwma(period, name ?? nameof(VWMA), cancellationToken);

    /// <summary>Returns the number of input bars consumed before <see cref="VWMA"/> produces a value.</summary>
    /// <param name="period">The averaging period.</param>
    /// <returns>The lookback count.</returns>
    [Obsolete("Use VwmaLookback instead.")]
    public static int VWMA_Lookback(int period = 20) => VwmaLookback(period);

    /// <summary>
    /// Creates a transform block that computes a simplified streaming zigzag.
    /// </summary>
    /// <param name="period">The minimum number of bars between pivots.</param>
    /// <param name="reversalPercent">The reversal threshold in percent.</param>
    /// <param name="name">The indicator key prefix. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    [Obsolete("Use Zigzag instead.")]
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ZIGZAG(
        int period,
        decimal reversalPercent = 5m,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        Zigzag(period, reversalPercent, name ?? nameof(ZIGZAG), cancellationToken);

    /// <summary>Returns the number of input bars consumed before <see cref="ZIGZAG"/> produces a value.</summary>
    /// <param name="period">The minimum bars between pivots.</param>
    /// <returns>Always zero.</returns>
    [Obsolete("Use ZigzagLookback instead.")]
    public static int ZIGZAG_Lookback(int period = 20) => ZigzagLookback(period);
#pragma warning restore S1133

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

    private static decimal TrueRange(decimal high, decimal low, decimal previousClose) =>
        Math.Max(high - low, Math.Max(Math.Abs(high - previousClose), Math.Abs(low - previousClose)));
}
