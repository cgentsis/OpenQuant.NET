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

    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// Double Exponential Moving Average (DEMA): <c>2 × EMA(n) − EMA(EMA(n))</c>.
    /// This reduces the lag inherent in a standard EMA.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points for the EMA smoothing factor.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DEMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var k = 2m / (period + 1);
        var count = 0;
        var sum = 0m;
        var ema = 0m;
        var emaOfEmaCount = 0;
        var emaOfEmaSum = 0m;
        var emaOfEma = 0m;

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
                    }
                }
                else
                {
                    ema = (enriched.Candle.Close * k) + (ema * (1 - k));
                }

                if (count >= period)
                {
                    emaOfEmaCount++;

                    if (emaOfEmaCount <= period)
                    {
                        emaOfEmaSum += ema;

                        if (emaOfEmaCount == period)
                        {
                            emaOfEma = emaOfEmaSum / period;
                            enriched.Indicators[name] = (2m * ema) - emaOfEma;
                        }
                    }
                    else
                    {
                        emaOfEma = (ema * k) + (emaOfEma * (1 - k));
                        enriched.Indicators[name] = (2m * ema) - emaOfEma;
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
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// Triple Exponential Moving Average (TEMA): <c>3×EMA − 3×EMA(EMA) + EMA(EMA(EMA))</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points for the EMA smoothing factor.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TEMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var k = 2m / (period + 1);
        var ema1Count = 0;
        var ema1Sum = 0m;
        var ema1 = 0m;
        var ema2Count = 0;
        var ema2Sum = 0m;
        var ema2 = 0m;
        var ema3Count = 0;
        var ema3Sum = 0m;
        var ema3 = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                ema1Count++;

                if (ema1Count <= period)
                {
                    ema1Sum += enriched.Candle.Close;
                    if (ema1Count == period)
                    {
                        ema1 = ema1Sum / period;
                    }
                }
                else
                {
                    ema1 = (enriched.Candle.Close * k) + (ema1 * (1 - k));
                }

                if (ema1Count >= period)
                {
                    ema2Count++;

                    if (ema2Count <= period)
                    {
                        ema2Sum += ema1;
                        if (ema2Count == period)
                        {
                            ema2 = ema2Sum / period;
                        }
                    }
                    else
                    {
                        ema2 = (ema1 * k) + (ema2 * (1 - k));
                    }
                }

                if (ema2Count >= period)
                {
                    ema3Count++;

                    if (ema3Count <= period)
                    {
                        ema3Sum += ema2;
                        if (ema3Count == period)
                        {
                            ema3 = ema3Sum / period;
                            enriched.Indicators[name] = (3m * ema1) - (3m * ema2) + ema3;
                        }
                    }
                    else
                    {
                        ema3 = (ema2 * k) + (ema3 * (1 - k));
                        enriched.Indicators[name] = (3m * ema1) - (3m * ema2) + ema3;
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
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// T3 (Triple Exponential Moving Average with volume factor). Uses six cascaded EMA stages
    /// with the specified <paramref name="volumeFactor"/> (default 0.7).
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points for the EMA smoothing factor.</param>
    /// <param name="volumeFactor">The volume factor (default 0.7). Higher values produce smoother output.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> T3(
        string name,
        int period,
        decimal volumeFactor = 0.7m,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var k = 2m / (period + 1);
        var v = volumeFactor;
        var c1 = -(v * v * v);
        var c2 = (3m * v * v) + (3m * v * v * v);
        var c3 = (-6m * v * v) - (3m * v) - (3m * v * v * v);
        var c4 = 1m + (3m * v) + (v * v * v) + (3m * v * v);

        var emas = new decimal[6];
        var counts = new int[6];
        var sums = new decimal[6];

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var input = enriched.Candle.Close;

                for (var i = 0; i < 6; i++)
                {
                    if (i > 0 && counts[i - 1] < period)
                    {
                        break;
                    }

                    counts[i]++;

                    if (counts[i] <= period)
                    {
                        sums[i] += input;

                        if (counts[i] == period)
                        {
                            emas[i] = sums[i] / period;
                        }
                    }
                    else
                    {
                        emas[i] = (input * k) + (emas[i] * (1 - k));
                    }

                    input = emas[i];
                }

                if (counts[5] >= period)
                {
                    enriched.Indicators[name] = (c1 * emas[5]) + (c2 * emas[4]) +
                                                (c3 * emas[3]) + (c4 * emas[2]);
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
    /// Triangular Moving Average (TRIMA): an SMA of an SMA, giving extra weight to the middle
    /// of the period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points for the outer window. Must be at least 2.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TRIMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var innerPeriod = (period / 2) + 1;
        var outerPeriod = period % 2 == 0 ? period / 2 : innerPeriod;

        var innerWindow = new Queue<decimal>(innerPeriod);
        var innerSum = 0m;

        var outerWindow = new Queue<decimal>(outerPeriod);
        var outerSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                innerSum += enriched.Candle.Close;
                innerWindow.Enqueue(enriched.Candle.Close);

                if (innerWindow.Count > innerPeriod)
                {
                    innerSum -= innerWindow.Dequeue();
                }

                if (innerWindow.Count == innerPeriod)
                {
                    var innerSma = innerSum / innerPeriod;

                    outerSum += innerSma;
                    outerWindow.Enqueue(innerSma);

                    if (outerWindow.Count > outerPeriod)
                    {
                        outerSum -= outerWindow.Dequeue();
                    }

                    if (outerWindow.Count == outerPeriod)
                    {
                        enriched.Indicators[name] = outerSum / outerPeriod;
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
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes
    /// Kaufman's Adaptive Moving Average (KAMA). KAMA adjusts its smoothing factor based on
    /// the efficiency ratio of the price series. When the market is trending, it acts like a
    /// short EMA; when choppy, it acts like a long EMA.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The efficiency ratio lookback period. Must be at least 2.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KAMA(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var fastSc = 2m / (2 + 1);
        var slowSc = 2m / (30 + 1);
        var window = new Queue<decimal>(period + 1);
        var kama = 0m;
        var initialized = false;

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
                    if (!initialized)
                    {
                        kama = enriched.Candle.Close;
                        initialized = true;
                        enriched.Indicators[name] = kama;
                    }
                    else
                    {
                        var prices = window.ToArray();
                        var direction = Math.Abs(prices[period] - prices[0]);
                        var volatility = 0m;

                        for (var i = 1; i <= period; i++)
                        {
                            volatility += Math.Abs(prices[i] - prices[i - 1]);
                        }

                        var er = volatility == 0m ? 0m : direction / volatility;
                        var sc = (er * (fastSc - slowSc)) + slowSc;
                        sc *= sc;

                        kama += sc * (enriched.Candle.Close - kama);
                        enriched.Indicators[name] = kama;
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
