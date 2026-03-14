using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides statistical function calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. These indicators compute statistics
/// over a rolling window of recent values.
/// </summary>
public static class StatisticFunctions
{
    /// <summary>
    /// Creates a transform block that computes the population variance of closing prices
    /// over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Var(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var sumY = 0m;
        var sumY2 = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var y = enriched.Candle.Close;
                sumY += y;
                sumY2 += y * y;
                window.Enqueue(y);

                if (window.Count > period)
                {
                    var old = window.Dequeue();
                    sumY -= old;
                    sumY2 -= old * old;
                }

                if (window.Count == period)
                {
                    var n = (decimal)period;
                    enriched.Indicators[name] = (sumY2 / n) - ((sumY / n) * (sumY / n));
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
    /// Creates a transform block that computes the standard deviation of closing prices
    /// over the specified period, optionally scaled by <paramref name="nbDev"/>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="nbDev">Number of deviations (multiplier). Defaults to 1.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> StdDev(
        string name,
        int period,
        decimal nbDev = 1m,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var sumY = 0m;
        var sumY2 = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var y = enriched.Candle.Close;
                sumY += y;
                sumY2 += y * y;
                window.Enqueue(y);

                if (window.Count > period)
                {
                    var old = window.Dequeue();
                    sumY -= old;
                    sumY2 -= old * old;
                }

                if (window.Count == period)
                {
                    var n = (decimal)period;
                    var variance = (sumY2 / n) - ((sumY / n) * (sumY / n));
                    enriched.Indicators[name] = (decimal)Math.Sqrt((double)variance) * nbDev;
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
    /// Creates a transform block that computes the linear regression value (the y-value at the
    /// end of the least-squares regression line) over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LinearReg(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var window = new Queue<decimal>(period);

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
                    var (slope, intercept) = ComputeLinearRegression(window, period);
                    enriched.Indicators[name] = intercept + (slope * (period - 1));
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
    /// Creates a transform block that computes the angle of the linear regression line
    /// in degrees: <c>atan(slope) × 180 / π</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LinearRegAngle(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var window = new Queue<decimal>(period);

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
                    var (slope, _) = ComputeLinearRegression(window, period);
                    enriched.Indicators[name] = (decimal)(Math.Atan((double)slope) * (180.0 / Math.PI));
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
    /// Creates a transform block that computes the y-intercept of the linear regression line
    /// over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LinearRegIntercept(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var window = new Queue<decimal>(period);

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
                    var (_, intercept) = ComputeLinearRegression(window, period);
                    enriched.Indicators[name] = intercept;
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
    /// Creates a transform block that computes the slope of the linear regression line
    /// over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> LinearRegSlope(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var window = new Queue<decimal>(period);

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
                    var (slope, _) = ComputeLinearRegression(window, period);
                    enriched.Indicators[name] = slope;
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
    /// Creates a transform block that computes the Time Series Forecast: the linear regression
    /// value projected one step ahead, i.e. <c>intercept + slope × period</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tsf(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var window = new Queue<decimal>(period);

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
                    var (slope, intercept) = ComputeLinearRegression(window, period);
                    enriched.Indicators[name] = intercept + (slope * period);
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
    /// Creates a transform block that computes the Pearson correlation coefficient between
    /// two price series over the specified period.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="seriesA">Selector for the first series value from each candle.</param>
    /// <param name="seriesB">Selector for the second series value from each candle.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Correl(
        string name,
        int period,
        Func<EnrichedCandle, decimal> seriesA,
        Func<EnrichedCandle, decimal> seriesB,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var xWindow = new Queue<decimal>(period);
        var yWindow = new Queue<decimal>(period);
        var sumX = 0m;
        var sumY = 0m;
        var sumXY = 0m;
        var sumX2 = 0m;
        var sumY2 = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var x = seriesA(enriched);
                var y = seriesB(enriched);

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumX2 += x * x;
                sumY2 += y * y;
                xWindow.Enqueue(x);
                yWindow.Enqueue(y);

                if (xWindow.Count > period)
                {
                    var oldX = xWindow.Dequeue();
                    var oldY = yWindow.Dequeue();
                    sumX -= oldX;
                    sumY -= oldY;
                    sumXY -= oldX * oldY;
                    sumX2 -= oldX * oldX;
                    sumY2 -= oldY * oldY;
                }

                if (xWindow.Count == period)
                {
                    var n = (decimal)period;
                    var numerator = (n * sumXY) - (sumX * sumY);
                    var denomA = (n * sumX2) - (sumX * sumX);
                    var denomB = (n * sumY2) - (sumY * sumY);
                    var denom = (double)(denomA * denomB);

                    enriched.Indicators[name] = denom <= 0.0
                        ? 0m
                        : numerator / (decimal)Math.Sqrt(denom);
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
    /// Creates a transform block that computes the Beta coefficient between an asset series
    /// and a benchmark series: <c>Cov(asset, benchmark) / Var(benchmark)</c>.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="assetSeries">Selector for the asset series value from each candle.</param>
    /// <param name="benchmarkSeries">Selector for the benchmark series value from each candle.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Beta(
        string name,
        int period,
        Func<EnrichedCandle, decimal> assetSeries,
        Func<EnrichedCandle, decimal> benchmarkSeries,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var xWindow = new Queue<decimal>(period);
        var yWindow = new Queue<decimal>(period);
        var sumX = 0m;
        var sumY = 0m;
        var sumXY = 0m;
        var sumY2 = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var x = assetSeries(enriched);
                var y = benchmarkSeries(enriched);

                sumX += x;
                sumY += y;
                sumXY += x * y;
                sumY2 += y * y;
                xWindow.Enqueue(x);
                yWindow.Enqueue(y);

                if (xWindow.Count > period)
                {
                    var oldX = xWindow.Dequeue();
                    var oldY = yWindow.Dequeue();
                    sumX -= oldX;
                    sumY -= oldY;
                    sumXY -= oldX * oldY;
                    sumY2 -= oldY * oldY;
                }

                if (xWindow.Count == period)
                {
                    var n = (decimal)period;
                    var covariance = (n * sumXY) - (sumX * sumY);
                    var varianceB = (n * sumY2) - (sumY * sumY);

                    enriched.Indicators[name] = varianceB == 0m
                        ? 0m
                        : covariance / varianceB;
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    private static (decimal Slope, decimal Intercept) ComputeLinearRegression(Queue<decimal> window, int period)
    {
        var n = (decimal)period;
        var sumX = n * (n - 1) / 2m;
        var sumX2 = n * (n - 1) * ((2m * n) - 1) / 6m;

        var sumY = 0m;
        var sumXY = 0m;
        var x = 0m;

        foreach (var y in window)
        {
            sumY += y;
            sumXY += x * y;
            x++;
        }

        var denom = (n * sumX2) - (sumX * sumX);

        if (denom == 0m)
        {
            return (0m, sumY / n);
        }

        var slope = ((n * sumXY) - (sumX * sumY)) / denom;
        var intercept = (sumY - (slope * sumX)) / n;

        return (slope, intercept);
    }
}
