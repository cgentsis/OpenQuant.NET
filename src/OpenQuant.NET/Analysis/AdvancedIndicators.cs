using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides advanced indicator calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages, including Parabolic SAR
/// and Hilbert Transform indicators.
/// </summary>
public static class AdvancedIndicators
{
    /// <summary>
    /// Creates a transform block that computes the Parabolic SAR (Stop and Reverse).
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="acceleration">The initial and increment acceleration factor (default 0.02).</param>
    /// <param name="maximum">The maximum acceleration factor (default 0.2).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sar(
        string name,
        decimal acceleration = 0.02m,
        decimal maximum = 0.2m,
        CancellationToken cancellationToken = default)
    {
        var count = 0;
        var isLong = true;
        var sar = 0m;
        var ep = 0m;
        var af = acceleration;
        var prevHigh = 0m;
        var prevLow = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                count++;

                if (count == 1)
                {
                    prevHigh = c.High;
                    prevLow = c.Low;
                    return enriched;
                }

                if (count == 2)
                {
                    isLong = c.Close >= prevLow;
                    sar = isLong ? prevLow : prevHigh;
                    ep = isLong ? c.High : c.Low;
                    af = acceleration;
                    enriched.Indicators[name] = sar;
                    prevHigh = c.High;
                    prevLow = c.Low;
                    return enriched;
                }

                var newSar = sar + (af * (ep - sar));

                if (isLong)
                {
                    newSar = Math.Min(newSar, Math.Min(prevLow, c.Low));

                    if (newSar > c.Low)
                    {
                        isLong = false;
                        newSar = ep;
                        ep = c.Low;
                        af = acceleration;
                    }
                    else
                    {
                        if (c.High > ep)
                        {
                            ep = c.High;
                            af = Math.Min(af + acceleration, maximum);
                        }
                    }
                }
                else
                {
                    newSar = Math.Max(newSar, Math.Max(prevHigh, c.High));

                    if (newSar < c.High)
                    {
                        isLong = true;
                        newSar = ep;
                        ep = c.High;
                        af = acceleration;
                    }
                    else
                    {
                        if (c.Low < ep)
                        {
                            ep = c.Low;
                            af = Math.Min(af + acceleration, maximum);
                        }
                    }
                }

                sar = newSar;
                enriched.Indicators[name] = sar;
                prevHigh = c.High;
                prevLow = c.Low;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the Hilbert Transform - Instantaneous Trendline.
    /// This is a smoothed version of the price using the Hilbert Transform approach.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtTrendline(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 7)
                {
                    var idx = prices.Count - 1;
                    var trendline = ((4m * price) +
                                    (3m * prices[idx - 1]) +
                                    (2m * prices[idx - 2]) +
                                    prices[idx - 3]) / 10m;
                    enriched.Indicators[name] = trendline;
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
    /// Creates a transform block that computes the Hilbert Transform - Dominant Cycle Period.
    /// Uses a simplified approach to estimate the dominant cycle period from price data.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtDcPeriod(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();
        var smoothPeriod = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 10)
                {
                    var idx = prices.Count - 1;
                    var detrender = ((0.0962m * prices[idx]) +
                                    (0.5769m * prices[idx - 2]) -
                                    (0.5769m * prices[idx - 4]) -
                                    (0.0962m * prices[idx - 6])) * ((0.075m * smoothPeriod) + 0.54m);

                    var period = detrender == 0m ? 15m : Math.Abs(prices[idx] / detrender);
                    period = Math.Max(6m, Math.Min(50m, period));
                    smoothPeriod = (0.33m * period) + (0.67m * smoothPeriod);

                    enriched.Indicators[name] = smoothPeriod;
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
    /// Creates a transform block that computes the Hilbert Transform - Dominant Cycle Phase in degrees.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtDcPhase(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();
        var smoothPeriod = 15m;
        var phase = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 10)
                {
                    var idx = prices.Count - 1;

                    var realPart = 0m;
                    var imagPart = 0m;
                    var dcPeriod = (int)Math.Round((double)smoothPeriod);
                    dcPeriod = Math.Max(6, Math.Min(50, dcPeriod));

                    if (idx >= dcPeriod)
                    {
                        for (var i = 0; i < dcPeriod; i++)
                        {
                            var angle = (decimal)((2.0 * Math.PI * i) / dcPeriod);
                            realPart += prices[idx - i] * (decimal)Math.Cos((double)angle);
                            imagPart += prices[idx - i] * (decimal)Math.Sin((double)angle);
                        }
                    }

                    if (Math.Abs(realPart) > 0.001m)
                    {
                        phase = (decimal)(Math.Atan((double)(imagPart / realPart)) * (180.0 / Math.PI));

                        if (realPart < 0m)
                        {
                            phase += 180m;
                        }
                    }

                    enriched.Indicators[name] = phase;
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
    /// Creates a transform block that computes the Hilbert Transform - Phasor Components.
    /// Outputs <c>{name}_InPhase</c> and <c>{name}_Quadrature</c>.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtPhasor(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();
        var smoothPeriod = 15m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 10)
                {
                    var idx = prices.Count - 1;
                    var dcPeriod = (int)Math.Round((double)smoothPeriod);
                    dcPeriod = Math.Max(6, Math.Min(50, dcPeriod));

                    var realPart = 0m;
                    var imagPart = 0m;

                    if (idx >= dcPeriod)
                    {
                        for (var i = 0; i < dcPeriod; i++)
                        {
                            var angle = (decimal)((2.0 * Math.PI * i) / dcPeriod);
                            realPart += prices[idx - i] * (decimal)Math.Cos((double)angle);
                            imagPart += prices[idx - i] * (decimal)Math.Sin((double)angle);
                        }
                    }

                    enriched.Indicators[name + "_InPhase"] = realPart;
                    enriched.Indicators[name + "_Quadrature"] = imagPart;
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
    /// Creates a transform block that computes the Hilbert Transform - SineWave.
    /// Outputs <c>{name}_Sine</c> and <c>{name}_LeadSine</c>.
    /// </summary>
    /// <param name="name">The key prefix for the output values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtSine(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();
        var smoothPeriod = 15m;
        var phase = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 10)
                {
                    var idx = prices.Count - 1;
                    var dcPeriod = (int)Math.Round((double)smoothPeriod);
                    dcPeriod = Math.Max(6, Math.Min(50, dcPeriod));

                    var realPart = 0m;
                    var imagPart = 0m;

                    if (idx >= dcPeriod)
                    {
                        for (var i = 0; i < dcPeriod; i++)
                        {
                            var angle = (decimal)((2.0 * Math.PI * i) / dcPeriod);
                            realPart += prices[idx - i] * (decimal)Math.Cos((double)angle);
                            imagPart += prices[idx - i] * (decimal)Math.Sin((double)angle);
                        }
                    }

                    if (Math.Abs(realPart) > 0.001m)
                    {
                        phase = (decimal)(Math.Atan((double)(imagPart / realPart)) * (180.0 / Math.PI));

                        if (realPart < 0m)
                        {
                            phase += 180m;
                        }
                    }

                    var phaseRadians = (double)phase * Math.PI / 180.0;
                    enriched.Indicators[name + "_Sine"] = (decimal)Math.Sin(phaseRadians);
                    enriched.Indicators[name + "_LeadSine"] = (decimal)Math.Sin(phaseRadians + (Math.PI / 4.0));
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
    /// Creates a transform block that computes the Hilbert Transform - Trend vs Cycle Mode.
    /// Outputs 1 when trending, 0 when in cycle mode.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> HtTrendMode(
        string name,
        CancellationToken cancellationToken = default)
    {
        var prices = new List<decimal>();

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var price = (enriched.Candle.High + enriched.Candle.Low) / 2m;
                prices.Add(price);

                if (prices.Count >= 10)
                {
                    var idx = prices.Count - 1;
                    var trendline = ((4m * prices[idx]) +
                                    (3m * prices[idx - 1]) +
                                    (2m * prices[idx - 2]) +
                                    prices[idx - 3]) / 10m;

                    var trend = Math.Abs(price - trendline) > (0.015m * price) ? 1m : 0m;
                    enriched.Indicators[name] = trend;
                }

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

    /// <summary>Returns the number of input bars consumed before <see cref="Sar"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int SarLookback() => 1;

    /// <summary>Returns the number of input bars consumed before <see cref="HtTrendline"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtTrendlineLookback() => 6;

    /// <summary>Returns the number of input bars consumed before <see cref="HtDcPeriod"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtDcPeriodLookback() => 32;

    /// <summary>Returns the number of input bars consumed before <see cref="HtDcPhase"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtDcPhaseLookback() => 32;

    /// <summary>Returns the number of input bars consumed before <see cref="HtPhasor"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtPhasorLookback() => 32;

    /// <summary>Returns the number of input bars consumed before <see cref="HtSine"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtSineLookback() => 32;

    /// <summary>Returns the number of input bars consumed before <see cref="HtTrendMode"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int HtTrendModeLookback() => 32;
#pragma warning restore S3400
}
