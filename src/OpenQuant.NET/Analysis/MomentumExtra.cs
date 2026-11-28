using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides additional momentum and trend-strength indicators as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Results are stored in
/// <see cref="EnrichedCandle.Indicators"/> under the method name by default.
/// </summary>
public static class MomentumExtra
{
    /// <summary>
    /// Creates a transform block that computes the Awesome Oscillator: the difference between a
    /// fast and a slow SMA of the median price.
    /// </summary>
    /// <param name="period">The fast period.</param>
    /// <param name="slowPeriod">The slow period. Defaults to 34.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AwesomeOscillator(
        int period,
        int slowPeriod = 34,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);
        var key = name ?? nameof(AwesomeOscillator);
        var medians = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(medians, (candle.High + candle.Low) / 2m, slowPeriod);

                if (medians.Count == slowPeriod)
                {
                    var fast = medians.TakeLast(period).Average();
                    var slow = medians.Average();
                    enriched.Indicators[key] = fast - slow;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="AwesomeOscillator"/> produces a value.</summary>
    /// <param name="period">The fast period.</param>
    /// <param name="slowPeriod">The slow period.</param>
    /// <returns>The lookback count.</returns>
    public static int AwesomeOscillatorLookback(int period = 14, int slowPeriod = 34) => slowPeriod - 1;

    /// <summary>
    /// Creates a transform block that computes the Accelerator Oscillator: the Awesome Oscillator
    /// minus an SMA of itself.
    /// </summary>
    /// <param name="period">The fast AO period.</param>
    /// <param name="slowPeriod">The slow AO period. Defaults to 34.</param>
    /// <param name="signalPeriod">The AO smoothing period. Defaults to 5.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AcceleratorOscillator(
        int period,
        int slowPeriod = 34,
        int signalPeriod = 5,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(signalPeriod, 1);
        var key = name ?? nameof(AcceleratorOscillator);
        var medians = new Queue<decimal>();
        var aoValues = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var candle = enriched.Candle;
                Enqueue(medians, (candle.High + candle.Low) / 2m, slowPeriod);

                if (medians.Count == slowPeriod)
                {
                    Enqueue(aoValues, medians.TakeLast(period).Average() - medians.Average(), signalPeriod);
                    if (aoValues.Count == signalPeriod)
                    {
                        enriched.Indicators[key] = aoValues.Last() - aoValues.Average();
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="AcceleratorOscillator"/> produces a value.</summary>
    /// <param name="period">The fast AO period.</param>
    /// <param name="slowPeriod">The slow AO period.</param>
    /// <param name="signalPeriod">The AO smoothing period.</param>
    /// <returns>The lookback count.</returns>
    public static int AcceleratorOscillatorLookback(int period = 14, int slowPeriod = 34, int signalPeriod = 5) =>
        slowPeriod + signalPeriod - 2;

    /// <summary>
    /// Creates a transform block that computes the Centre of Gravity oscillator, a negatively
    /// signed weighted average of the most recent closing prices.
    /// </summary>
    /// <param name="period">The lookback period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CenterOfGravity(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(CenterOfGravity);
        var closes = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                Enqueue(closes, enriched.Candle.Close, period);
                if (closes.Count == period)
                {
                    var sum = closes.Sum();
                    if (sum != 0m)
                    {
                        var weightedSum = 0m;
                        var weight = 1;
                        foreach (var close in closes)
                        {
                            weightedSum += weight * close;
                            weight++;
                        }

                        enriched.Indicators[key] = -weightedSum / sum;
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="CenterOfGravity"/> produces a value.</summary>
    /// <param name="period">The lookback period.</param>
    /// <returns>The lookback count.</returns>
    public static int CenterOfGravityLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the Choppiness Index from the sum of true ranges
    /// and the high-low price range over a window.
    /// </summary>
    /// <param name="period">The window period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndex(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateChoppiness(period, name, cancellationToken, nameof(ChoppinessIndex), useTrueRangeBox: false);

    /// <summary>Returns the number of input bars consumed before <see cref="ChoppinessIndex"/> produces a value.</summary>
    /// <param name="period">The window period.</param>
    /// <returns>The lookback count.</returns>
    public static int ChoppinessIndexLookback(int period = 14) => period;

    /// <summary>
    /// Creates a transform block that computes a Choppiness Index variant whose denominator is the
    /// spread between the largest and smallest true range in the window.
    /// </summary>
    /// <param name="period">The window period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndexTrueRangeBox(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default) =>
        CreateChoppiness(period, name, cancellationToken, nameof(ChoppinessIndexTrueRangeBox), useTrueRangeBox: true);

    /// <summary>Returns the number of input bars consumed before <see cref="ChoppinessIndexTrueRangeBox"/> produces a value.</summary>
    /// <param name="period">The window period.</param>
    /// <returns>The lookback count.</returns>
    public static int ChoppinessIndexTrueRangeBoxLookback(int period = 14) => period;

    /// <summary>
    /// Creates a transform block that computes the Coppock Curve: the weighted sum of the combined
    /// long and short rate-of-change values.
    /// </summary>
    /// <param name="period">The weighting period.</param>
    /// <param name="longPeriod">The long rate-of-change period. Defaults to 11.</param>
    /// <param name="shortPeriod">The short rate-of-change period. Defaults to 14.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CoppockCurve(
        int period,
        int longPeriod = 11,
        int shortPeriod = 14,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(longPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(shortPeriod, 1);
        var key = name ?? nameof(CoppockCurve);
        var lookback = Math.Max(longPeriod, shortPeriod);
        var closes = new Queue<decimal>();
        var rawValues = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                Enqueue(closes, close, lookback + 1);

                if (closes.Count == lookback + 1)
                {
                    var longPast = closes.ElementAt(closes.Count - 1 - longPeriod);
                    var shortPast = closes.ElementAt(closes.Count - 1 - shortPeriod);
                    if (longPast != 0m && shortPast != 0m)
                    {
                        var raw = (((close / longPast) - 1m) * 100m) + (((close / shortPast) - 1m) * 100m);
                        Enqueue(rawValues, raw, period);

                        if (rawValues.Count == period)
                        {
                            enriched.Indicators[key] = Helpers.ComputeWMA(rawValues.ToArray(), period);
                        }
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="CoppockCurve"/> produces a value.</summary>
    /// <param name="period">The weighting period.</param>
    /// <param name="longPeriod">The long rate-of-change period.</param>
    /// <param name="shortPeriod">The short rate-of-change period.</param>
    /// <returns>The lookback count.</returns>
    public static int CoppockCurveLookback(int period = 14, int longPeriod = 11, int shortPeriod = 14) =>
        Math.Max(longPeriod, shortPeriod) + period - 1;

    /// <summary>
    /// Creates a transform block that computes Connors RSI: the average of a Wilder RSI of price,
    /// a Wilder RSI of the up/down streak and a Wilder RSI of the streak magnitude.
    /// </summary>
    /// <param name="period">The price RSI period.</param>
    /// <param name="rsiPeriod">The streak RSI period. Defaults to 3.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ConnorsRSI(
        int period,
        int rsiPeriod = 3,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(rsiPeriod, 1);
        var key = name ?? nameof(ConnorsRSI);
        var priceRsi = new WilderRsi(period);
        var streakRsi = new WilderRsi(rsiPeriod);
        var magnitudeRsi = new WilderRsi(rsiPeriod);
        var previousClose = 0m;
        var streak = 0;
        var seeded = false;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                if (!seeded)
                {
                    seeded = true;
                }
                else
                {
                    if (close > previousClose)
                    {
                        streak = streak > 0 ? streak + 1 : 1;
                    }
                    else if (close < previousClose)
                    {
                        streak = streak < 0 ? streak - 1 : -1;
                    }
                    else
                    {
                        streak = 0;
                    }
                }

                var priceValue = priceRsi.Update(close);
                var streakValue = streakRsi.Update(streak);
                var magnitudeValue = magnitudeRsi.Update(Math.Abs(streak));
                previousClose = close;

                if (priceValue.HasValue && streakValue.HasValue && magnitudeValue.HasValue)
                {
                    enriched.Indicators[key] = (priceValue.Value + streakValue.Value + magnitudeValue.Value) / 3m;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="ConnorsRSI"/> produces a value.</summary>
    /// <param name="period">The price RSI period.</param>
    /// <param name="rsiPeriod">The streak RSI period.</param>
    /// <returns>The lookback count.</returns>
    public static int ConnorsRSILookback(int period = 14, int rsiPeriod = 3) => Math.Max(period, rsiPeriod) + 1;

    /// <summary>
    /// Creates a transform block that computes the Pearson correlation between closing prices and
    /// the time index over a window. Zero-variance windows are skipped.
    /// </summary>
    /// <param name="period">The window period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CorrelationTrendIndicator(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(CorrelationTrendIndicator);
        var closes = new Queue<decimal>();
        var timeIndices = Enumerable.Range(0, period).Select(index => (decimal)index).ToArray();
        var meanTime = timeIndices.Average();
        var timeVariance = timeIndices.Sum(index => (index - meanTime) * (index - meanTime));

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                Enqueue(closes, enriched.Candle.Close, period);
                if (closes.Count == period && timeVariance != 0m)
                {
                    var values = closes.ToArray();
                    var meanClose = values.Average();
                    var covariance = 0m;
                    var closeVariance = 0m;

                    for (var index = 0; index < period; index++)
                    {
                        var closeDelta = values[index] - meanClose;
                        covariance += closeDelta * (timeIndices[index] - meanTime);
                        closeVariance += closeDelta * closeDelta;
                    }

                    if (closeVariance != 0m)
                    {
                        enriched.Indicators[key] = covariance /
                            (decimal)Math.Sqrt((double)(closeVariance * timeVariance));
                    }
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="CorrelationTrendIndicator"/> produces a value.</summary>
    /// <param name="period">The window period.</param>
    /// <returns>The lookback count.</returns>
    public static int CorrelationTrendIndicatorLookback(int period = 14) => period - 1;

    /// <summary>
    /// Creates a transform block that computes the Derivative Oscillator: the difference between a
    /// fast and a slow SMA of closing prices.
    /// </summary>
    /// <param name="period">The fast period.</param>
    /// <param name="slowPeriod">The slow period. Defaults to 8.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DerivativeOscillator(
        int period,
        int slowPeriod = 8,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);
        var key = name ?? nameof(DerivativeOscillator);
        var closes = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var capacity = Math.Max(period, slowPeriod);
                Enqueue(closes, enriched.Candle.Close, capacity);
                if (closes.Count == capacity)
                {
                    enriched.Indicators[key] = closes.TakeLast(period).Average() - closes.TakeLast(slowPeriod).Average();
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="DerivativeOscillator"/> produces a value.</summary>
    /// <param name="period">The fast period.</param>
    /// <param name="slowPeriod">The slow period.</param>
    /// <returns>The lookback count.</returns>
    public static int DerivativeOscillatorLookback(int period = 14, int slowPeriod = 8) =>
        Math.Max(period, slowPeriod) - 1;

    /// <summary>
    /// Creates a transform block that computes the Detrended Price Oscillator by comparing a past
    /// closing price with the SMA centred on that bar.
    /// </summary>
    /// <param name="period">The SMA period.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DetrendedPriceOscillator(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(DetrendedPriceOscillator);
        var shift = (period / 2) + 1;
        var capacity = period + shift;
        var closes = new Queue<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                Enqueue(closes, enriched.Candle.Close, capacity);
                if (closes.Count == capacity)
                {
                    var values = closes.ToArray();
                    var pastClose = values[period - 1];
                    var pastAverage = values.Take(period).Average();
                    enriched.Indicators[key] = pastClose - pastAverage;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="DetrendedPriceOscillator"/> produces a value.</summary>
    /// <param name="period">The SMA period.</param>
    /// <returns>The lookback count.</returns>
    public static int DetrendedPriceOscillatorLookback(int period = 14) => period + (period / 2);

    private static TransformBlock<EnrichedCandle, EnrichedCandle> CreateChoppiness(
        int period,
        string? name,
        CancellationToken cancellationToken,
        string defaultName,
        bool useTrueRangeBox)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? defaultName;
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
                    Enqueue(trueRanges, TrueRange(candle.High, candle.Low, previousClose.Value), period);
                    Enqueue(highs, candle.High, period);
                    Enqueue(lows, candle.Low, period);

                    if (trueRanges.Count == period)
                    {
                        var denominator = useTrueRangeBox
                            ? trueRanges.Max() - trueRanges.Min()
                            : highs.Max() - lows.Min();

                        if (denominator > 0m)
                        {
                            var value = 100m *
                                (decimal)(Math.Log10((double)(trueRanges.Sum() / denominator)) /
                                          Math.Log10((double)period));
                            enriched.Indicators[key] = value;
                        }
                    }
                }

                previousClose = candle.Close;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

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

    private sealed class WilderRsi
    {
        private readonly int _period;
        private decimal? _previousValue;
        private decimal _averageGain;
        private decimal _averageLoss;
        private int _changesSeen;
        private bool _seeded;

        public WilderRsi(int period)
        {
            _period = period;
        }

        public decimal? Update(decimal value)
        {
            if (!_previousValue.HasValue)
            {
                _previousValue = value;
                return null;
            }

            var change = value - _previousValue.Value;
            _previousValue = value;
            var gain = change > 0m ? change : 0m;
            var loss = change < 0m ? -change : 0m;
            _changesSeen++;

            if (!_seeded)
            {
                _averageGain += gain;
                _averageLoss += loss;
                if (_changesSeen == _period)
                {
                    _averageGain /= _period;
                    _averageLoss /= _period;
                    _seeded = true;
                }

                return _seeded ? Calculate() : null;
            }

            _averageGain = ((_averageGain * (_period - 1)) + gain) / _period;
            _averageLoss = ((_averageLoss * (_period - 1)) + loss) / _period;
            return Calculate();
        }

        private decimal Calculate()
        {
            if (_averageLoss == 0m)
            {
                return _averageGain == 0m ? 0m : 100m;
            }

            return 100m - (100m / (1m + (_averageGain / _averageLoss)));
        }
    }
}
