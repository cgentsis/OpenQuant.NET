using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides volume-based indicator calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages.
/// </summary>
public static class VolumeIndicators
{
    /// <summary>
    /// Creates a transform block that computes On Balance Volume (OBV): a cumulative total
    /// of volume where volume is added on up-closes and subtracted on down-closes.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Obv(
        string name,
        CancellationToken cancellationToken = default)
    {
        decimal? prevClose = null;
        var obv = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevClose.HasValue)
                {
                    if (c.Close > prevClose.Value)
                    {
                        obv += c.Volume;
                    }
                    else if (c.Close < prevClose.Value)
                    {
                        obv -= c.Volume;
                    }
                }

                enriched.Indicators[name] = obv;
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
    /// Creates a transform block that computes the Chaikin A/D Line:
    /// cumulative sum of <c>((Close − Low) − (High − Close)) / (High − Low) × Volume</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ad(
        string name,
        CancellationToken cancellationToken = default)
    {
        var adLine = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var range = c.High - c.Low;

                if (range != 0m)
                {
                    var clv = ((c.Close - c.Low) - (c.High - c.Close)) / range;
                    adLine += clv * c.Volume;
                }

                enriched.Indicators[name] = adLine;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the Chaikin A/D Oscillator (ADOSC):
    /// <c>EMA(fastPeriod, AD) − EMA(slowPeriod, AD)</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="fastPeriod">The fast EMA period (default 3).</param>
    /// <param name="slowPeriod">The slow EMA period (default 10).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AdOsc(
        string name,
        int fastPeriod = 3,
        int slowPeriod = 10,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);

        var adLine = 0m;
        var fastK = 2m / (fastPeriod + 1);
        var slowK = 2m / (slowPeriod + 1);
        var fastCount = 0;
        var slowCount = 0;
        var fastSum = 0m;
        var slowSum = 0m;
        var fastEma = 0m;
        var slowEma = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var range = c.High - c.Low;

                if (range != 0m)
                {
                    var clv = ((c.Close - c.Low) - (c.High - c.Close)) / range;
                    adLine += clv * c.Volume;
                }

                fastCount++;
                slowCount++;

                var fastReady = false;
                var slowReady = false;

                if (fastCount <= fastPeriod)
                {
                    fastSum += adLine;

                    if (fastCount == fastPeriod)
                    {
                        fastEma = fastSum / fastPeriod;
                        fastReady = true;
                    }
                }
                else
                {
                    fastEma = (adLine * fastK) + (fastEma * (1 - fastK));
                    fastReady = true;
                }

                if (slowCount <= slowPeriod)
                {
                    slowSum += adLine;

                    if (slowCount == slowPeriod)
                    {
                        slowEma = slowSum / slowPeriod;
                        slowReady = true;
                    }
                }
                else
                {
                    slowEma = (adLine * slowK) + (slowEma * (1 - slowK));
                    slowReady = true;
                }

                if (fastReady && slowReady)
                {
                    enriched.Indicators[name] = fastEma - slowEma;
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
    /// Creates a transform block that computes the Money Flow Index (MFI):
    /// <c>100 − 100 / (1 + MoneyFlowRatio)</c> over the specified period.
    /// MFI is a volume-weighted RSI.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Mfi(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevTp = null;
        var posMf = new Queue<decimal>(period);
        var negMf = new Queue<decimal>(period);
        var posSum = 0m;
        var negSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var tp = (c.High + c.Low + c.Close) / 3m;

                if (prevTp.HasValue)
                {
                    var rawMf = tp * c.Volume;
                    var pos = tp > prevTp.Value ? rawMf : 0m;
                    var neg = tp < prevTp.Value ? rawMf : 0m;

                    posSum += pos;
                    negSum += neg;
                    posMf.Enqueue(pos);
                    negMf.Enqueue(neg);

                    if (posMf.Count > period)
                    {
                        posSum -= posMf.Dequeue();
                        negSum -= negMf.Dequeue();
                    }

                    if (posMf.Count == period)
                    {
                        enriched.Indicators[name] = negSum == 0m
                            ? 100m
                            : 100m - (100m / (1m + (posSum / negSum)));
                    }
                }

                prevTp = tp;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

#pragma warning disable S3400 // Lookback API requires methods even when the result is constant.

    // Lookback methods

    /// <summary>Returns the number of input bars consumed before <see cref="Obv"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int ObvLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="Ad"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int AdLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="AdOsc"/> produces its first value.</summary>
    /// <param name="fastPeriod">The fast period.</param>
    /// <param name="slowPeriod">The slow period.</param>
    /// <returns>The lookback count.</returns>
    public static int AdOscLookback(int fastPeriod = 3, int slowPeriod = 10) => slowPeriod - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="Mfi"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int MfiLookback(int period = 14) => period;
#pragma warning restore S3400
}
