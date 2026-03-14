using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides Stochastic oscillator calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages.
/// </summary>
public static class StochasticIndicators
{
    /// <summary>
    /// Creates a transform block that computes the Stochastic Oscillator.
    /// Outputs <c>{name}_K</c> (slow %K, an SMA-smoothed fast %K) and <c>{name}_D</c> (SMA of slow %K).
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="fastKPeriod">The lookback period for raw %K (default 5).</param>
    /// <param name="slowKPeriod">The SMA period for smoothing %K (default 3).</param>
    /// <param name="slowDPeriod">The SMA period for %D signal line (default 3).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Stoch(
        string name,
        int fastKPeriod = 5,
        int slowKPeriod = 3,
        int slowDPeriod = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastKPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowKPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowDPeriod, 1);

        var highs = new Queue<decimal>(fastKPeriod);
        var lows = new Queue<decimal>(fastKPeriod);
        var kWindow = new Queue<decimal>(slowKPeriod);
        var kSum = 0m;
        var dWindow = new Queue<decimal>(slowDPeriod);
        var dSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                highs.Enqueue(c.High);
                lows.Enqueue(c.Low);

                if (highs.Count > fastKPeriod)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == fastKPeriod)
                {
                    var hh = highs.Max();
                    var ll = lows.Min();
                    var range = hh - ll;
                    var fastK = range == 0m ? 50m : ((c.Close - ll) / range) * 100m;

                    kSum += fastK;
                    kWindow.Enqueue(fastK);

                    if (kWindow.Count > slowKPeriod)
                    {
                        kSum -= kWindow.Dequeue();
                    }

                    if (kWindow.Count == slowKPeriod)
                    {
                        var slowK = kSum / slowKPeriod;
                        enriched.Indicators[name + "_K"] = slowK;

                        dSum += slowK;
                        dWindow.Enqueue(slowK);

                        if (dWindow.Count > slowDPeriod)
                        {
                            dSum -= dWindow.Dequeue();
                        }

                        if (dWindow.Count == slowDPeriod)
                        {
                            enriched.Indicators[name + "_D"] = dSum / slowDPeriod;
                        }
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
    /// Creates a transform block that computes the Fast Stochastic Oscillator.
    /// Outputs <c>{name}_K</c> (raw fast %K) and <c>{name}_D</c> (SMA of fast %K).
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="fastKPeriod">The lookback period for %K (default 5).</param>
    /// <param name="fastDPeriod">The SMA period for %D (default 3).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> StochF(
        string name,
        int fastKPeriod = 5,
        int fastDPeriod = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastKPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(fastDPeriod, 1);

        var highs = new Queue<decimal>(fastKPeriod);
        var lows = new Queue<decimal>(fastKPeriod);
        var dWindow = new Queue<decimal>(fastDPeriod);
        var dSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                highs.Enqueue(c.High);
                lows.Enqueue(c.Low);

                if (highs.Count > fastKPeriod)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == fastKPeriod)
                {
                    var hh = highs.Max();
                    var ll = lows.Min();
                    var range = hh - ll;
                    var fastK = range == 0m ? 50m : ((c.Close - ll) / range) * 100m;

                    enriched.Indicators[name + "_K"] = fastK;

                    dSum += fastK;
                    dWindow.Enqueue(fastK);

                    if (dWindow.Count > fastDPeriod)
                    {
                        dSum -= dWindow.Dequeue();
                    }

                    if (dWindow.Count == fastDPeriod)
                    {
                        enriched.Indicators[name + "_D"] = dSum / fastDPeriod;
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
    /// Creates a transform block that computes the Stochastic RSI: the Stochastic formula
    /// applied to RSI values instead of price. Outputs <c>{name}_K</c> and <c>{name}_D</c>.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="rsiPeriod">The RSI period (default 14).</param>
    /// <param name="stochPeriod">The Stochastic lookback period over RSI values (default 14).</param>
    /// <param name="kPeriod">The SMA period for %K smoothing (default 3).</param>
    /// <param name="dPeriod">The SMA period for %D (default 3).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> StochRsi(
        string name,
        int rsiPeriod = 14,
        int stochPeriod = 14,
        int kPeriod = 3,
        int dPeriod = 3,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(rsiPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(stochPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(kPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(dPeriod, 1);

        decimal? prevClose = null;
        var rsiCount = 0;
        var gainSum = 0m;
        var lossSum = 0m;
        var avgGain = 0m;
        var avgLoss = 0m;

        var rsiWindow = new Queue<decimal>(stochPeriod);
        var kWindow = new Queue<decimal>(kPeriod);
        var kSum = 0m;
        var dWindow = new Queue<decimal>(dPeriod);
        var dSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;

                if (prevClose.HasValue)
                {
                    var change = close - prevClose.Value;
                    var gain = change > 0 ? change : 0m;
                    var loss = change < 0 ? -change : 0m;
                    rsiCount++;

                    decimal? rsi = null;

                    if (rsiCount <= rsiPeriod)
                    {
                        gainSum += gain;
                        lossSum += loss;

                        if (rsiCount == rsiPeriod)
                        {
                            avgGain = gainSum / rsiPeriod;
                            avgLoss = lossSum / rsiPeriod;
                            rsi = avgLoss == 0m ? 100m : 100m - (100m / (1m + (avgGain / avgLoss)));
                        }
                    }
                    else
                    {
                        avgGain = ((avgGain * (rsiPeriod - 1)) + gain) / rsiPeriod;
                        avgLoss = ((avgLoss * (rsiPeriod - 1)) + loss) / rsiPeriod;
                        rsi = avgLoss == 0m ? 100m : 100m - (100m / (1m + (avgGain / avgLoss)));
                    }

                    if (rsi.HasValue)
                    {
                        rsiWindow.Enqueue(rsi.Value);

                        if (rsiWindow.Count > stochPeriod)
                        {
                            rsiWindow.Dequeue();
                        }

                        if (rsiWindow.Count == stochPeriod)
                        {
                            var rsiMax = rsiWindow.Max();
                            var rsiMin = rsiWindow.Min();
                            var range = rsiMax - rsiMin;
                            var fastK = range == 0m ? 50m : ((rsi.Value - rsiMin) / range) * 100m;

                            kSum += fastK;
                            kWindow.Enqueue(fastK);

                            if (kWindow.Count > kPeriod)
                            {
                                kSum -= kWindow.Dequeue();
                            }

                            if (kWindow.Count == kPeriod)
                            {
                                var slowK = kSum / kPeriod;
                                enriched.Indicators[name + "_K"] = slowK;

                                dSum += slowK;
                                dWindow.Enqueue(slowK);

                                if (dWindow.Count > dPeriod)
                                {
                                    dSum -= dWindow.Dequeue();
                                }

                                if (dWindow.Count == dPeriod)
                                {
                                    enriched.Indicators[name + "_D"] = dSum / dPeriod;
                                }
                            }
                        }
                    }
                }

                prevClose = close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    // Lookback methods

    /// <summary>Returns the number of input bars consumed before <see cref="Stoch"/> produces its first value.</summary>
    /// <param name="fastKPeriod">The fast K period.</param>
    /// <param name="slowKPeriod">The slow K period.</param>
    /// <param name="slowDPeriod">The slow D period.</param>
    /// <returns>The lookback count.</returns>
    public static int StochLookback(int fastKPeriod = 5, int slowKPeriod = 3, int slowDPeriod = 3) => (fastKPeriod - 1) + (slowKPeriod - 1) + (slowDPeriod - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="StochF"/> produces its first value.</summary>
    /// <param name="fastKPeriod">The fast K period.</param>
    /// <param name="fastDPeriod">The fast D period.</param>
    /// <returns>The lookback count.</returns>
    public static int StochFLookback(int fastKPeriod = 5, int fastDPeriod = 3) => (fastKPeriod - 1) + (fastDPeriod - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="StochRsi"/> produces its first value.</summary>
    /// <param name="rsiPeriod">The RSI period.</param>
    /// <param name="stochPeriod">The stochastic period.</param>
    /// <param name="kPeriod">The K period.</param>
    /// <param name="dPeriod">The D period.</param>
    /// <returns>The lookback count.</returns>
    public static int StochRsiLookback(int rsiPeriod = 14, int stochPeriod = 14, int kPeriod = 3, int dPeriod = 3) => rsiPeriod + (stochPeriod - 1) + (kPeriod - 1) + (dPeriod - 1);
}
