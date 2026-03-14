using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides momentum oscillator calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. These indicators measure the rate and
/// direction of price changes to identify overbought/oversold conditions and trend strength.
/// </summary>
public static class MomentumOscillators
{
    /// <summary>
    /// Creates a transform block that computes the Absolute Price Oscillator (APO):
    /// <c>fastEMA − slowEMA</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="fastPeriod">The fast EMA period.</param>
    /// <param name="slowPeriod">The slow EMA period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Apo(
        string name,
        int fastPeriod,
        int slowPeriod,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);

        var fastState = new EmaState(fastPeriod);
        var slowState = new EmaState(slowPeriod);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                var fastReady = fastState.Update(close);
                var slowReady = slowState.Update(close);

                if (fastReady && slowReady)
                {
                    enriched.Indicators[name] = fastState.Value - slowState.Value;
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
    /// Creates a transform block that computes the Percentage Price Oscillator (PPO):
    /// <c>((fastEMA − slowEMA) / slowEMA) × 100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="fastPeriod">The fast EMA period.</param>
    /// <param name="slowPeriod">The slow EMA period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ppo(
        string name,
        int fastPeriod,
        int slowPeriod,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);

        var fastState = new EmaState(fastPeriod);
        var slowState = new EmaState(slowPeriod);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                var fastReady = fastState.Update(close);
                var slowReady = slowState.Update(close);

                if (fastReady && slowReady && slowState.Value != 0m)
                {
                    enriched.Indicators[name] = ((fastState.Value - slowState.Value) / slowState.Value) * 100m;
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
    /// Creates a transform block that computes the MACD (Moving Average Convergence/Divergence).
    /// Outputs three values: <c>{name}_MACD</c>, <c>{name}_Signal</c>, and <c>{name}_Hist</c>.
    /// </summary>
    /// <param name="name">The key prefix for output values.</param>
    /// <param name="fastPeriod">The fast EMA period (default 12).</param>
    /// <param name="slowPeriod">The slow EMA period (default 26).</param>
    /// <param name="signalPeriod">The signal line EMA period (default 9).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Macd(
        string name,
        int fastPeriod = 12,
        int slowPeriod = 26,
        int signalPeriod = 9,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(fastPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(slowPeriod, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(signalPeriod, 1);

        var fastState = new EmaState(fastPeriod);
        var slowState = new EmaState(slowPeriod);
        var signalState = new EmaState(signalPeriod);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;
                var fastReady = fastState.Update(close);
                var slowReady = slowState.Update(close);

                if (fastReady && slowReady)
                {
                    var macdLine = fastState.Value - slowState.Value;
                    enriched.Indicators[name + "_MACD"] = macdLine;

                    if (signalState.Update(macdLine))
                    {
                        enriched.Indicators[name + "_Signal"] = signalState.Value;
                        enriched.Indicators[name + "_Hist"] = macdLine - signalState.Value;
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
    /// Creates a transform block that computes the Relative Strength Index (RSI).
    /// RSI = 100 − 100 / (1 + RS), where RS = avg gain / avg loss over the period.
    /// Uses Wilder's smoothing method (exponential-like with factor 1/period).
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Rsi(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevClose = null;
        var count = 0;
        var gainSum = 0m;
        var lossSum = 0m;
        var avgGain = 0m;
        var avgLoss = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;

                if (prevClose.HasValue)
                {
                    var change = close - prevClose.Value;
                    var gain = change > 0 ? change : 0m;
                    var loss = change < 0 ? -change : 0m;
                    count++;

                    if (count <= period)
                    {
                        gainSum += gain;
                        lossSum += loss;

                        if (count == period)
                        {
                            avgGain = gainSum / period;
                            avgLoss = lossSum / period;
                            enriched.Indicators[name] = avgLoss == 0m
                                ? 100m
                                : 100m - (100m / (1m + (avgGain / avgLoss)));
                        }
                    }
                    else
                    {
                        avgGain = ((avgGain * (period - 1)) + gain) / period;
                        avgLoss = ((avgLoss * (period - 1)) + loss) / period;
                        enriched.Indicators[name] = avgLoss == 0m
                            ? 100m
                            : 100m - (100m / (1m + (avgGain / avgLoss)));
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

    /// <summary>
    /// Creates a transform block that computes the Chande Momentum Oscillator (CMO):
    /// <c>(sumUp − sumDown) / (sumUp + sumDown) × 100</c> over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cmo(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevClose = null;
        var gains = new Queue<decimal>(period);
        var losses = new Queue<decimal>(period);
        var sumUp = 0m;
        var sumDown = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var close = enriched.Candle.Close;

                if (prevClose.HasValue)
                {
                    var change = close - prevClose.Value;
                    var gain = change > 0 ? change : 0m;
                    var loss = change < 0 ? -change : 0m;

                    sumUp += gain;
                    sumDown += loss;
                    gains.Enqueue(gain);
                    losses.Enqueue(loss);

                    if (gains.Count > period)
                    {
                        sumUp -= gains.Dequeue();
                        sumDown -= losses.Dequeue();
                    }

                    if (gains.Count == period)
                    {
                        var total = sumUp + sumDown;
                        enriched.Indicators[name] = total == 0m ? 0m : ((sumUp - sumDown) / total) * 100m;
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

    /// <summary>
    /// Creates a transform block that computes the Commodity Channel Index (CCI):
    /// <c>(TypicalPrice − SMA(TP)) / (0.015 × MeanDeviation)</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cci(
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
                var c = enriched.Candle;
                var tp = (c.High + c.Low + c.Close) / 3m;

                sum += tp;
                window.Enqueue(tp);

                if (window.Count > period)
                {
                    sum -= window.Dequeue();
                }

                if (window.Count == period)
                {
                    var mean = sum / period;
                    var meanDev = 0m;

                    foreach (var v in window)
                    {
                        meanDev += Math.Abs(v - mean);
                    }

                    meanDev /= period;
                    enriched.Indicators[name] = meanDev == 0m ? 0m : (tp - mean) / (0.015m * meanDev);
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
    /// Creates a transform block that computes Williams %R:
    /// <c>(HighestHigh − Close) / (HighestHigh − LowestLow) × −100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> WillR(
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
                var c = enriched.Candle;
                highs.Enqueue(c.High);
                lows.Enqueue(c.Low);

                if (highs.Count > period)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == period)
                {
                    var hh = highs.Max();
                    var ll = lows.Min();
                    var range = hh - ll;
                    enriched.Indicators[name] = range == 0m ? 0m : ((hh - c.Close) / range) * -100m;
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
    /// Creates a transform block that computes the Ultimate Oscillator using three time periods
    /// (default 7, 14, 28) with weights 4, 2, 1.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period1">Short period (default 7).</param>
    /// <param name="period2">Medium period (default 14).</param>
    /// <param name="period3">Long period (default 28).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when any period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> UltOsc(
        string name,
        int period1 = 7,
        int period2 = 14,
        int period3 = 28,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period1, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(period2, 1);
        ArgumentOutOfRangeException.ThrowIfLessThan(period3, 1);

        decimal? prevClose = null;
        var bpWindow = new Queue<decimal>(period3);
        var trWindow = new Queue<decimal>(period3);
        var bpSum = 0m;
        var trSum = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevClose.HasValue)
                {
                    var truelow = Math.Min(c.Low, prevClose.Value);
                    var truehigh = Math.Max(c.High, prevClose.Value);
                    var bp = c.Close - truelow;
                    var tr = truehigh - truelow;

                    bpSum += bp;
                    trSum += tr;
                    bpWindow.Enqueue(bp);
                    trWindow.Enqueue(tr);

                    if (bpWindow.Count > period3)
                    {
                        bpSum -= bpWindow.Dequeue();
                        trSum -= trWindow.Dequeue();
                    }

                    if (bpWindow.Count >= period3)
                    {
                        var bp1 = bpWindow.Skip(period3 - period1).Sum();
                        var tr1 = trWindow.Skip(period3 - period1).Sum();
                        var bp2 = bpWindow.Skip(period3 - period2).Sum();
                        var tr2 = trWindow.Skip(period3 - period2).Sum();

                        var avg1 = tr1 == 0m ? 0m : bp1 / tr1;
                        var avg2 = tr2 == 0m ? 0m : bp2 / tr2;
                        var avg3 = trSum == 0m ? 0m : bpSum / trSum;

                        enriched.Indicators[name] = (((4m * avg1) + (2m * avg2) + avg3) / 7m) * 100m;
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
    /// Creates a transform block that computes TRIX: the 1-day rate of change of a triple-smoothed EMA.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The EMA period for each of the three smoothing stages.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Trix(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var ema1 = new EmaState(period);
        var ema2 = new EmaState(period);
        var ema3 = new EmaState(period);
        decimal? prevTripleEma = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                if (ema1.Update(enriched.Candle.Close) && ema2.Update(ema1.Value) && ema3.Update(ema2.Value))
                {
                    if (prevTripleEma.HasValue && prevTripleEma.Value != 0m)
                    {
                        enriched.Indicators[name] = ((ema3.Value - prevTripleEma.Value) / prevTripleEma.Value) * 100m;
                    }

                    prevTripleEma = ema3.Value;
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
    /// Creates a transform block that computes the Aroon indicator, outputting
    /// <c>{name}_Up</c> and <c>{name}_Down</c>.
    /// Aroon Up = ((period − bars since highest high) / period) × 100.
    /// Aroon Down = ((period − bars since lowest low) / period) × 100.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Aroon(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var highs = new Queue<decimal>(period + 1);
        var lows = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                highs.Enqueue(enriched.Candle.High);
                lows.Enqueue(enriched.Candle.Low);

                if (highs.Count > period + 1)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == period + 1)
                {
                    var highIdx = 0;
                    var highMax = decimal.MinValue;
                    var lowIdx = 0;
                    var lowMin = decimal.MaxValue;
                    var idx = 0;

                    foreach (var h in highs)
                    {
                        if (h >= highMax)
                        {
                            highMax = h;
                            highIdx = idx;
                        }

                        idx++;
                    }

                    idx = 0;
                    foreach (var l in lows)
                    {
                        if (l <= lowMin)
                        {
                            lowMin = l;
                            lowIdx = idx;
                        }

                        idx++;
                    }

                    enriched.Indicators[name + "_Up"] = ((decimal)highIdx / period) * 100m;
                    enriched.Indicators[name + "_Down"] = ((decimal)lowIdx / period) * 100m;
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
    /// Creates a transform block that computes the Aroon Oscillator: Aroon Up − Aroon Down.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The lookback period.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AroonOsc(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var highs = new Queue<decimal>(period + 1);
        var lows = new Queue<decimal>(period + 1);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                highs.Enqueue(enriched.Candle.High);
                lows.Enqueue(enriched.Candle.Low);

                if (highs.Count > period + 1)
                {
                    highs.Dequeue();
                    lows.Dequeue();
                }

                if (highs.Count == period + 1)
                {
                    var highIdx = 0;
                    var highMax = decimal.MinValue;
                    var lowIdx = 0;
                    var lowMin = decimal.MaxValue;
                    var idx = 0;

                    foreach (var h in highs)
                    {
                        if (h >= highMax)
                        {
                            highMax = h;
                            highIdx = idx;
                        }

                        idx++;
                    }

                    idx = 0;
                    foreach (var l in lows)
                    {
                        if (l <= lowMin)
                        {
                            lowMin = l;
                            lowIdx = idx;
                        }

                        idx++;
                    }

                    var aroonUp = ((decimal)highIdx / period) * 100m;
                    var aroonDown = ((decimal)lowIdx / period) * 100m;
                    enriched.Indicators[name] = aroonUp - aroonDown;
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
    /// Encapsulates EMA computation state, reusable across multiple indicators.
    /// </summary>
    private sealed class EmaState
    {
        private readonly int _period;
        private readonly decimal _k;
        private int _count;
        private decimal _sum;

        public EmaState(int period)
        {
            _period = period;
            _k = 2m / (period + 1);
        }

        public decimal Value { get; private set; }

        public bool Update(decimal input)
        {
            _count++;

            if (_count <= _period)
            {
                _sum += input;

                if (_count == _period)
                {
                    Value = _sum / _period;
                    return true;
                }

                return false;
            }

            Value = (input * _k) + (Value * (1 - _k));
            return true;
        }
    }
}
