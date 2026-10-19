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

        return IndicatorFactory.Create(name, period, 
            enriched =>
            {
                // SMA specific implementation
                var window = new Queue<decimal>(period);
                var sum = 0m;
                
                // Try to get existing state for reuse when available
                if (enriched.Indicators.TryGetValue($"SMA_WINDOW_{name}", out var windowObj))
                {
                    window = (Queue<decimal>)windowObj;
                    sum = (decimal)enriched.Indicators[$"SMA_SUM_{name}"];
                }

                sum += enriched.Candle.Close;
                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period)
                {
                    sum -= window.Dequeue();
                }

                // Store updated state for next iteration
                enriched.Indicators[$"SMA_WINDOW_{name}"] = window;
                enriched.Indicators[$"SMA_SUM_{name}"] = sum;

                if (window.Count == period)
                {
                    return sum / period;
                }
                
                return 0m; // No value yet
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // EMA specific implementation
                var count = 0;
                var sum = 0m;
                var ema = 0m;
                var k = 2m / (period + 1);

                // Try to get existing state for reuse when available
                if (enriched.Indicators.TryGetValue($"EMA_COUNT_{name}", out var countObj))
                {
                    count = (int)countObj;
                }

                if (enriched.Indicators.TryGetValue($"EMA_SUM_{name}", out var sumObj))
                {
                    sum = (decimal)sumObj;
                }

                if (enriched.Indicators.TryGetValue($"EMA_EMA_{name}", out var emaObj))
                {
                    ema = (decimal)emaObj;
                }

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
                    ema = Helpers.ComputeEMA(ema, enriched.Candle.Close, k);
                }

                // Store updated state for next iteration
                enriched.Indicators[$"EMA_COUNT_{name}"] = count;
                enriched.Indicators[$"EMA_SUM_{name}"] = sum;
                enriched.Indicators[$"EMA_EMA_{name}"] = ema;

                return ema;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // WMA specific implementation
                var window = new Queue<decimal>(period);
                var divisor = period * (period + 1) / 2m;

                // Try to get existing state for reuse when available
                if (enriched.Indicators.TryGetValue($"WMA_WINDOW_{name}", out var windowObj))
                {
                    window = (Queue<decimal>)windowObj;
                }

                window.Enqueue(enriched.Candle.Close);

                if (window.Count > period)
                {
                    window.Dequeue();
                }

                // Store updated state for next iteration
                enriched.Indicators[$"WMA_WINDOW_{name}"] = window;

                if (window.Count == period)
                {
                    var weightedSum = 0m;
                    var weight = 1;

                    foreach (var price in window)
                    {
                        weightedSum += price * weight;
                        weight++;
                    }

                    return weightedSum / divisor;
                }
                
                return 0m; // No value yet
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // HMA specific implementation
                var halfPeriod = period / 2;
                var sqrtPeriod = Math.Max(1, (int)Math.Round(Math.Sqrt(period)));

                // Get state
                if (!enriched.Indicators.TryGetValue($"HMA_PRICE_BUFFER_{name}", out var priceBufferObj))
                {
                    priceBufferObj = new decimal[period];
                    enriched.Indicators[$"HMA_PRICE_INDEX_{name}"] = 0;
                    enriched.Indicators[$"HMA_PRICE_COUNT_{name}"] = 0;
                }
                
                if (!enriched.Indicators.TryGetValue($"HMA_DIFF_BUFFER_{name}", out var diffBufferObj))
                {
                    diffBufferObj = new decimal[sqrtPeriod];
                    enriched.Indicators[$"HMA_DIFF_INDEX_{name}"] = 0;
                    enriched.Indicators[$"HMA_DIFF_COUNT_{name}"] = 0;
                }
                
                var priceBuffer = (decimal[])priceBufferObj;
                var diffBuffer = (decimal[])diffBufferObj;
                var priceIndex = (int)enriched.Indicators.GetValueOrDefault($"HMA_PRICE_INDEX_{name}", 0);
                var priceCount = (int)enriched.Indicators.GetValueOrDefault($"HMA_PRICE_COUNT_{name}", 0);
                var diffIndex = (int)enriched.Indicators.GetValueOrDefault($"HMA_DIFF_INDEX_{name}", 0);
                var diffCount = (int)enriched.Indicators.GetValueOrDefault($"HMA_DIFF_COUNT_{name}", 0);

                // Update price buffer
                priceBuffer[priceIndex] = enriched.Candle.Close;
                priceIndex = (priceIndex + 1) % period;
                priceCount = Math.Min(priceCount + 1, period);
                
                decimal value = 0m;
                
                if (priceCount == period)
                {
                    var wmaFull = Helpers.ComputeWMACircular(priceBuffer, priceIndex, period);
                    var wmaHalf = Helpers.ComputeWMACircular(priceBuffer, (priceIndex + period - halfPeriod) % period, halfPeriod);
                    var diff = (2m * wmaHalf) - wmaFull;

                    // Update diff buffer
                    diffBuffer[diffIndex] = diff;
                    diffIndex = (diffIndex + 1) % sqrtPeriod;
                    diffCount = Math.Min(diffCount + 1, sqrtPeriod);

                    if (diffCount == sqrtPeriod)
                    {
                        value = Helpers.ComputeWMACircular(diffBuffer, diffIndex, sqrtPeriod);
                    }
                }

                // Store updated state
                enriched.Indicators[$"HMA_PRICE_BUFFER_{name}"] = priceBuffer;
                enriched.Indicators[$"HMA_PRICE_INDEX_{name}"] = priceIndex;
                enriched.Indicators[$"HMA_PRICE_COUNT_{name}"] = priceCount;
                enriched.Indicators[$"HMA_DIFF_BUFFER_{name}"] = diffBuffer;
                enriched.Indicators[$"HMA_DIFF_INDEX_{name}"] = diffIndex;
                enriched.Indicators[$"HMA_DIFF_COUNT_{name}"] = diffCount;

                return value;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // DEMA specific implementation
                var k = 2m / (period + 1);
                var count = 0;
                var sum = 0m;
                var ema = 0m;
                var emaOfEmaCount = 0;
                var emaOfEmaSum = 0m;
                var emaOfEma = 0m;

                // Try to get existing state for reuse when available
                if (enriched.Indicators.TryGetValue($"DEMA_COUNT_{name}", out var countObj))
                {
                    count = (int)countObj;
                }

                if (enriched.Indicators.TryGetValue($"DEMA_SUM_{name}", out var sumObj))
                {
                    sum = (decimal)sumObj;
                }

                if (enriched.Indicators.TryGetValue($"DEMA_EMA_{name}", out var emaObj))
                {
                    ema = (decimal)emaObj;
                }

                if (enriched.Indicators.TryGetValue($"DEMA_EMA_OF_EMA_COUNT_{name}", out var emaOfEmaCountObj))
                {
                    emaOfEmaCount = (int)emaOfEmaCountObj;
                }

                if (enriched.Indicators.TryGetValue($"DEMA_EMA_OF_EMA_SUM_{name}", out var emaOfEmaSumObj))
                {
                    emaOfEmaSum = (decimal)emaOfEmaSumObj;
                }

                if (enriched.Indicators.TryGetValue($"DEMA_EMA_OF_EMA_{name}", out var emaOfEmaObj))
                {
                    emaOfEma = (decimal)emaOfEmaObj;
                }

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
                    ema = Helpers.ComputeEMA(ema, enriched.Candle.Close, k);
                }

                decimal value = 0m;
                
                if (count >= period)
                {
                    emaOfEmaCount++;

                    if (emaOfEmaCount <= period)
                    {
                        emaOfEmaSum += ema;

                        if (emaOfEmaCount == period)
                        {
                            emaOfEma = emaOfEmaSum / period;
                            value = (2m * ema) - emaOfEma;
                        }
                    }
                    else
                    {
                        emaOfEma = Helpers.ComputeEMA(emaOfEma, ema, k);
                        value = (2m * ema) - emaOfEma;
                    }
                }

                // Store updated state for next iteration
                enriched.Indicators[$"DEMA_COUNT_{name}"] = count;
                enriched.Indicators[$"DEMA_SUM_{name}"] = sum;
                enriched.Indicators[$"DEMA_EMA_{name}"] = ema;
                enriched.Indicators[$"DEMA_EMA_OF_EMA_COUNT_{name}"] = emaOfEmaCount;
                enriched.Indicators[$"DEMA_EMA_OF_EMA_SUM_{name}"] = emaOfEmaSum;
                enriched.Indicators[$"DEMA_EMA_OF_EMA_{name}"] = emaOfEma;

                return value;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // TEMA specific implementation
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

                // Try to get existing state for reuse when available
                if (enriched.Indicators.TryGetValue($"TEMA_EMA1_COUNT_{name}", out var ema1CountObj))
                {
                    ema1Count = (int)ema1CountObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA1_SUM_{name}", out var ema1SumObj))
                {
                    ema1Sum = (decimal)ema1SumObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA1_{name}", out var ema1Obj))
                {
                    ema1 = (decimal)ema1Obj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA2_COUNT_{name}", out var ema2CountObj))
                {
                    ema2Count = (int)ema2CountObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA2_SUM_{name}", out var ema2SumObj))
                {
                    ema2Sum = (decimal)ema2SumObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA2_{name}", out var ema2Obj))
                {
                    ema2 = (decimal)ema2Obj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA3_COUNT_{name}", out var ema3CountObj))
                {
                    ema3Count = (int)ema3CountObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA3_SUM_{name}", out var ema3SumObj))
                {
                    ema3Sum = (decimal)ema3SumObj;
                }

                if (enriched.Indicators.TryGetValue($"TEMA_EMA3_{name}", out var ema3Obj))
                {
                    ema3 = (decimal)ema3Obj;
                }

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
                    ema1 = Helpers.ComputeEMA(ema1, enriched.Candle.Close, k);
                }

                decimal value = 0m;

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
                        ema2 = Helpers.ComputeEMA(ema2, ema1, k);
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
                            value = (3m * ema1) - (3m * ema2) + ema3;
                        }
                    }
                    else
                    {
                        ema3 = Helpers.ComputeEMA(ema3, ema2, k);
                        value = (3m * ema1) - (3m * ema2) + ema3;
                    }
                }

                // Store updated state for next iteration
                enriched.Indicators[$"TEMA_EMA1_COUNT_{name}"] = ema1Count;
                enriched.Indicators[$"TEMA_EMA1_SUM_{name}"] = ema1Sum;
                enriched.Indicators[$"TEMA_EMA1_{name}"] = ema1;
                enriched.Indicators[$"TEMA_EMA2_COUNT_{name}"] = ema2Count;
                enriched.Indicators[$"TEMA_EMA2_SUM_{name}"] = ema2Sum;
                enriched.Indicators[$"TEMA_EMA2_{name}"] = ema2;
                enriched.Indicators[$"TEMA_EMA3_COUNT_{name}"] = ema3Count;
                enriched.Indicators[$"TEMA_EMA3_SUM_{name}"] = ema3Sum;
                enriched.Indicators[$"TEMA_EMA3_{name}"] = ema3;

                return value;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // T3 specific implementation
                var emas = new decimal[6];
                var counts = new int[6];
                var sums = new decimal[6];

                // Try to get existing state from the candle
                for (var i = 0; i < 6; i++)
                {
                    if (!enriched.Indicators.TryGetValue($"T3_EMA_{name}_{i}", out var emaObj))
                    {
                        emas[i] = 0m;
                    }
                    else
                    {
                        emas[i] = (decimal)emaObj;
                    }
                    
                    if (!enriched.Indicators.TryGetValue($"T3_COUNT_{name}_{i}", out var countObj))
                    {
                        counts[i] = 0;
                    }
                    else
                    {
                        counts[i] = (int)countObj;
                    }
                    
                    if (!enriched.Indicators.TryGetValue($"T3_SUM_{name}_{i}", out var sumObj))
                    {
                        sums[i] = 0m;
                    }
                    else
                    {
                        sums[i] = (decimal)sumObj;
                    }
                }

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
                        emas[i] = Helpers.ComputeEMA(emas[i], input, k);
                    }

                    input = emas[i];
                }

                decimal value = 0m;

                if (counts[5] >= period)
                {
                    value = (c1 * emas[5]) + (c2 * emas[4]) +
                            (c3 * emas[3]) + (c4 * emas[2]);
                }

                // Store state back to the candle
                for (var i = 0; i < 6; i++)
                {
                    enriched.Indicators[$"T3_EMA_{name}_{i}"] = emas[i];
                    enriched.Indicators[$"T3_COUNT_{name}_{i}"] = counts[i];
                    enriched.Indicators[$"T3_SUM_{name}_{i}"] = sums[i];
                }

                return value;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // TRIMA specific implementation
                var innerPeriod = (period / 2) + 1;
                var outerPeriod = period % 2 == 0 ? period / 2 : innerPeriod;

                var innerWindow = new Queue<decimal>(innerPeriod);
                var innerSum = 0m;

                var outerWindow = new Queue<decimal>(outerPeriod);
                var outerSum = 0m;

                // Try to retrieve state from prior processing
                if (enriched.Indicators.TryGetValue($"TRIMA_INNER_WINDOW_{name}", out var innerWindowObj))
                {
                    innerWindow = (Queue<decimal>)innerWindowObj;
                    innerSum = (decimal)enriched.Indicators[$"TRIMA_INNER_SUM_{name}"];
                    
                    innerSum += enriched.Candle.Close;
                    innerWindow.Enqueue(enriched.Candle.Close);

                    if (innerWindow.Count > innerPeriod)
                    {
                        innerSum -= innerWindow.Dequeue();
                    }
                }
                else
                {
                    // Initialize state
                    innerWindow.Enqueue(enriched.Candle.Close);
                    innerSum = enriched.Candle.Close;
                }

                decimal value = 0m;

                if (innerWindow.Count == innerPeriod)
                {
                    var innerSma = innerSum / innerPeriod;

                    // Update outer window
                    if (enriched.Indicators.TryGetValue($"TRIMA_OUTER_WINDOW_{name}", out var outerWindowObj))
                    {
                        outerWindow = (Queue<decimal>)outerWindowObj;
                        outerSum = (decimal)enriched.Indicators[$"TRIMA_OUTER_SUM_{name}"];
                        
                        outerSum += innerSma;
                        outerWindow.Enqueue(innerSma);

                        if (outerWindow.Count > outerPeriod)
                        {
                            outerSum -= outerWindow.Dequeue();
                        }
                    }
                    else
                    {
                        // Initialize state
                        outerWindow.Enqueue(innerSma);
                        outerSum = innerSma;
                    }

                    if (outerWindow.Count == outerPeriod)
                    {
                        value = outerSum / outerPeriod;
                    }
                    
                    // Store state back to candle
                    enriched.Indicators[$"TRIMA_OUTER_WINDOW_{name}"] = outerWindow;
                    enriched.Indicators[$"TRIMA_OUTER_SUM_{name}"] = outerSum;
                }

                // Store state back to candle 
                enriched.Indicators[$"TRIMA_INNER_WINDOW_{name}"] = innerWindow;
                enriched.Indicators[$"TRIMA_INNER_SUM_{name}"] = innerSum;

                return value;
            }, cancellationToken);
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

        return IndicatorFactory.Create(name, period,
            enriched =>
            {
                // KAMA specific implementation
                var fastSc = 2m / (2 + 1);
                var slowSc = 2m / (30 + 1);
                var window = new Queue<decimal>(period + 1);
                var kama = 0m;
                var initialized = false;

                // Try to get existing state
                if (enriched.Indicators.TryGetValue($"KAMA_WINDOW_{name}", out var windowObj))
                {
                    window = (Queue<decimal>)windowObj;
                    window.Enqueue(enriched.Candle.Close);

                    if (window.Count > period + 1)
                    {
                        window.Dequeue();
                    }
                }
                else
                {
                    // Initialize with new values
                    window.Enqueue(enriched.Candle.Close);
                }

                if (enriched.Indicators.TryGetValue($"KAMA_KAMA_{name}", out var kamaObj))
                {
                    kama = (decimal)kamaObj;
                }

                if (enriched.Indicators.TryGetValue($"KAMA_INITIALIZED_{name}", out var initializedObj))
                {
                    initialized = (bool)initializedObj;
                }

                decimal value = 0m;

                if (window.Count == period + 1)
                {
                    if (!initialized)
                    {
                        kama = enriched.Candle.Close;
                        initialized = true;
                        value = kama;
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
                        value = kama;
                    }
                }

                // Store state back to the candle
                enriched.Indicators[$"KAMA_WINDOW_{name}"] = window;
                enriched.Indicators[$"KAMA_KAMA_{name}"] = kama;
                enriched.Indicators[$"KAMA_INITIALIZED_{name}"] = initialized;

                return value;
            }, cancellationToken);
    }

#pragma warning disable SA1202 // Lookback methods are intentionally kept at the end of the class.

    // Lookback methods

    /// <summary>Returns the number of input bars consumed before <see cref="SMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int SMALookback(int period) => period - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="EMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int EMALookback(int period) => period - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="WMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int WMALookback(int period) => period - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="HMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int HMALookback(int period) => (period - 1) + ((int)Math.Floor(Math.Sqrt(period)) - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="DEMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int DEMALookback(int period) => 2 * (period - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="TEMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int TEMALookback(int period) => 3 * (period - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="T3"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int T3Lookback(int period) => 6 * (period - 1);

    /// <summary>Returns the number of input bars consumed before <see cref="TRIMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int TRIMALookback(int period) => period - 1;

    /// <summary>Returns the number of input bars consumed before <see cref="KAMA"/> produces its first value.</summary>
    /// <param name="period">The period.</param>
    /// <returns>The lookback count.</returns>
    public static int KAMALookback(int period) => period;
#pragma warning restore SA1202
}