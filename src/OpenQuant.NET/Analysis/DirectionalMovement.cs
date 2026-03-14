using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides Wilder's Directional Movement System calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Indicators build upon each other:
/// +DM/−DM → +DI/−DI → DX → ADX → ADXR.
/// </summary>
public static class DirectionalMovement
{
    /// <summary>
    /// Creates a transform block that computes Plus Directional Movement (+DM) smoothed over the period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PlusDM(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        var count = 0;
        var sum = 0m;
        var smoothed = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var plusDm = upMove > downMove && upMove > 0 ? upMove : 0m;

                    count++;

                    if (count <= period)
                    {
                        sum += plusDm;

                        if (count == period)
                        {
                            smoothed = sum;
                            enriched.Indicators[name] = smoothed;
                        }
                    }
                    else
                    {
                        smoothed = smoothed - (smoothed / period) + plusDm;
                        enriched.Indicators[name] = smoothed;
                    }
                }

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
    /// Creates a transform block that computes Minus Directional Movement (−DM) smoothed over the period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MinusDM(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        var count = 0;
        var sum = 0m;
        var smoothed = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var minusDm = downMove > upMove && downMove > 0 ? downMove : 0m;

                    count++;

                    if (count <= period)
                    {
                        sum += minusDm;

                        if (count == period)
                        {
                            smoothed = sum;
                            enriched.Indicators[name] = smoothed;
                        }
                    }
                    else
                    {
                        smoothed = smoothed - (smoothed / period) + minusDm;
                        enriched.Indicators[name] = smoothed;
                    }
                }

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
    /// Creates a transform block that computes the Plus Directional Indicator (+DI):
    /// <c>(smoothed +DM / smoothed TR) × 100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PlusDI(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        decimal? prevClose = null;
        var count = 0;
        var dmSum = 0m;
        var trSum = 0m;
        var smoothDm = 0m;
        var smoothTr = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var plusDm = upMove > downMove && upMove > 0 ? upMove : 0m;

                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose!.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    count++;

                    if (count <= period)
                    {
                        dmSum += plusDm;
                        trSum += tr;

                        if (count == period)
                        {
                            smoothDm = dmSum;
                            smoothTr = trSum;
                            enriched.Indicators[name] = smoothTr == 0m ? 0m : (smoothDm / smoothTr) * 100m;
                        }
                    }
                    else
                    {
                        smoothDm = smoothDm - (smoothDm / period) + plusDm;
                        smoothTr = smoothTr - (smoothTr / period) + tr;
                        enriched.Indicators[name] = smoothTr == 0m ? 0m : (smoothDm / smoothTr) * 100m;
                    }
                }

                prevHigh = c.High;
                prevLow = c.Low;
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
    /// Creates a transform block that computes the Minus Directional Indicator (−DI):
    /// <c>(smoothed −DM / smoothed TR) × 100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MinusDI(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        decimal? prevClose = null;
        var count = 0;
        var dmSum = 0m;
        var trSum = 0m;
        var smoothDm = 0m;
        var smoothTr = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var minusDm = downMove > upMove && downMove > 0 ? downMove : 0m;

                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose!.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    count++;

                    if (count <= period)
                    {
                        dmSum += minusDm;
                        trSum += tr;

                        if (count == period)
                        {
                            smoothDm = dmSum;
                            smoothTr = trSum;
                            enriched.Indicators[name] = smoothTr == 0m ? 0m : (smoothDm / smoothTr) * 100m;
                        }
                    }
                    else
                    {
                        smoothDm = smoothDm - (smoothDm / period) + minusDm;
                        smoothTr = smoothTr - (smoothTr / period) + tr;
                        enriched.Indicators[name] = smoothTr == 0m ? 0m : (smoothDm / smoothTr) * 100m;
                    }
                }

                prevHigh = c.High;
                prevLow = c.Low;
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
    /// Creates a transform block that computes the Directional Movement Index (DX):
    /// <c>|+DI − −DI| / (+DI + −DI) × 100</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Dx(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        decimal? prevClose = null;
        var count = 0;
        var plusDmSum = 0m;
        var minusDmSum = 0m;
        var trSum = 0m;
        var smoothPlusDm = 0m;
        var smoothMinusDm = 0m;
        var smoothTr = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var plusDm = upMove > downMove && upMove > 0 ? upMove : 0m;
                    var minusDm = downMove > upMove && downMove > 0 ? downMove : 0m;

                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose!.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    count++;

                    if (count <= period)
                    {
                        plusDmSum += plusDm;
                        minusDmSum += minusDm;
                        trSum += tr;

                        if (count == period)
                        {
                            smoothPlusDm = plusDmSum;
                            smoothMinusDm = minusDmSum;
                            smoothTr = trSum;
                            ComputeDx(enriched, name, smoothPlusDm, smoothMinusDm, smoothTr);
                        }
                    }
                    else
                    {
                        smoothPlusDm = smoothPlusDm - (smoothPlusDm / period) + plusDm;
                        smoothMinusDm = smoothMinusDm - (smoothMinusDm / period) + minusDm;
                        smoothTr = smoothTr - (smoothTr / period) + tr;
                        ComputeDx(enriched, name, smoothPlusDm, smoothMinusDm, smoothTr);
                    }
                }

                prevHigh = c.High;
                prevLow = c.Low;
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
    /// Creates a transform block that computes the Average Directional Movement Index (ADX):
    /// a smoothed average of DX over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Adx(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        decimal? prevClose = null;
        var dmCount = 0;
        var plusDmSum = 0m;
        var minusDmSum = 0m;
        var trSum = 0m;
        var smoothPlusDm = 0m;
        var smoothMinusDm = 0m;
        var smoothTr = 0m;
        var dxCount = 0;
        var dxSum = 0m;
        var adx = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var plusDm = upMove > downMove && upMove > 0 ? upMove : 0m;
                    var minusDm = downMove > upMove && downMove > 0 ? downMove : 0m;

                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose!.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    dmCount++;

                    decimal dx;

                    if (dmCount <= period)
                    {
                        plusDmSum += plusDm;
                        minusDmSum += minusDm;
                        trSum += tr;

                        if (dmCount == period)
                        {
                            smoothPlusDm = plusDmSum;
                            smoothMinusDm = minusDmSum;
                            smoothTr = trSum;
                            dx = GetDxValue(smoothPlusDm, smoothMinusDm, smoothTr);
                            AccumulateAdx(enriched, name, dx, ref dxCount, ref dxSum, ref adx, period);
                        }
                    }
                    else
                    {
                        smoothPlusDm = smoothPlusDm - (smoothPlusDm / period) + plusDm;
                        smoothMinusDm = smoothMinusDm - (smoothMinusDm / period) + minusDm;
                        smoothTr = smoothTr - (smoothTr / period) + tr;
                        dx = GetDxValue(smoothPlusDm, smoothMinusDm, smoothTr);
                        AccumulateAdx(enriched, name, dx, ref dxCount, ref dxSum, ref adx, period);
                    }
                }

                prevHigh = c.High;
                prevLow = c.Low;
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
    /// Creates a transform block that computes the Average Directional Movement Index Rating (ADXR):
    /// <c>(currentADX + ADX[period bars ago]) / 2</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The smoothing period (default 14).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Adxr(
        string name,
        int period = 14,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        decimal? prevHigh = null;
        decimal? prevLow = null;
        decimal? prevClose = null;
        var dmCount = 0;
        var plusDmSum = 0m;
        var minusDmSum = 0m;
        var trSum = 0m;
        var smoothPlusDm = 0m;
        var smoothMinusDm = 0m;
        var smoothTr = 0m;
        var dxCount = 0;
        var dxSum = 0m;
        var adx = 0m;
        var adxHistory = new Queue<decimal>(period);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevHigh.HasValue)
                {
                    var upMove = c.High - prevHigh.Value;
                    var downMove = prevLow!.Value - c.Low;
                    var plusDm = upMove > downMove && upMove > 0 ? upMove : 0m;
                    var minusDm = downMove > upMove && downMove > 0 ? downMove : 0m;

                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose!.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    var tr = Math.Max(hl, Math.Max(hc, lc));

                    dmCount++;
                    decimal dx;

                    if (dmCount <= period)
                    {
                        plusDmSum += plusDm;
                        minusDmSum += minusDm;
                        trSum += tr;

                        if (dmCount == period)
                        {
                            smoothPlusDm = plusDmSum;
                            smoothMinusDm = minusDmSum;
                            smoothTr = trSum;
                            dx = GetDxValue(smoothPlusDm, smoothMinusDm, smoothTr);
                            UpdateAdxAndAdxr(enriched, name, dx, ref dxCount, ref dxSum, ref adx, adxHistory, period);
                        }
                    }
                    else
                    {
                        smoothPlusDm = smoothPlusDm - (smoothPlusDm / period) + plusDm;
                        smoothMinusDm = smoothMinusDm - (smoothMinusDm / period) + minusDm;
                        smoothTr = smoothTr - (smoothTr / period) + tr;
                        dx = GetDxValue(smoothPlusDm, smoothMinusDm, smoothTr);
                        UpdateAdxAndAdxr(enriched, name, dx, ref dxCount, ref dxSum, ref adx, adxHistory, period);
                    }
                }

                prevHigh = c.High;
                prevLow = c.Low;
                prevClose = c.Close;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    private static void ComputeDx(EnrichedCandle enriched, string name, decimal plusDm, decimal minusDm, decimal tr)
    {
        if (tr == 0m)
        {
            return;
        }

        var plusDi = (plusDm / tr) * 100m;
        var minusDi = (minusDm / tr) * 100m;
        var diSum = plusDi + minusDi;

        if (diSum != 0m)
        {
            enriched.Indicators[name] = (Math.Abs(plusDi - minusDi) / diSum) * 100m;
        }
    }

    private static decimal GetDxValue(decimal plusDm, decimal minusDm, decimal tr)
    {
        if (tr == 0m)
        {
            return 0m;
        }

        var plusDi = (plusDm / tr) * 100m;
        var minusDi = (minusDm / tr) * 100m;
        var diSum = plusDi + minusDi;

        return diSum == 0m ? 0m : (Math.Abs(plusDi - minusDi) / diSum) * 100m;
    }

    private static void AccumulateAdx(
        EnrichedCandle enriched,
        string name,
        decimal dx,
        ref int dxCount,
        ref decimal dxSum,
        ref decimal adx,
        int period)
    {
        dxCount++;

        if (dxCount <= period)
        {
            dxSum += dx;

            if (dxCount == period)
            {
                adx = dxSum / period;
                enriched.Indicators[name] = adx;
            }
        }
        else
        {
            adx = ((adx * (period - 1)) + dx) / period;
            enriched.Indicators[name] = adx;
        }
    }

    private static void UpdateAdxAndAdxr(
        EnrichedCandle enriched,
        string name,
        decimal dx,
        ref int dxCount,
        ref decimal dxSum,
        ref decimal adx,
        Queue<decimal> adxHistory,
        int period)
    {
        dxCount++;

        if (dxCount <= period)
        {
            dxSum += dx;

            if (dxCount == period)
            {
                adx = dxSum / period;
                adxHistory.Enqueue(adx);

                if (adxHistory.Count > period)
                {
                    var oldAdx = adxHistory.Dequeue();
                    enriched.Indicators[name] = (adx + oldAdx) / 2m;
                }
            }
        }
        else
        {
            adx = ((adx * (period - 1)) + dx) / period;
            adxHistory.Enqueue(adx);

            if (adxHistory.Count > period)
            {
                var oldAdx = adxHistory.Dequeue();
                enriched.Indicators[name] = (adx + oldAdx) / 2m;
            }
        }
    }

#pragma warning disable SA1202 // Lookback methods are intentionally kept at the end of the class.

    // Lookback methods

    /// <summary>Returns the number of input bars consumed before <see cref="PlusDM"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int PlusDMLookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="MinusDM"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int MinusDMLookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="PlusDI"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int PlusDILookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="MinusDI"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int MinusDILookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="Dx"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int DxLookback(int period = 14) => period;

    /// <summary>Returns the number of input bars consumed before <see cref="Adx"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int AdxLookback(int period = 14) => (2 * period) - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="Adxr"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int AdxrLookback(int period = 14) => (3 * period) - 2;
#pragma warning restore SA1202
}
