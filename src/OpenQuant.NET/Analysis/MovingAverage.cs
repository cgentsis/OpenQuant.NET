using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides moving average calculations as TPL Dataflow blocks.
/// </summary>
public static class MovingAverage
{
    /// <summary>
    /// Creates an <see cref="ActionBlock{Candle}"/> that computes the simple moving average (SMA)
    /// of closing prices over the specified period and forwards each result to <paramref name="target"/>.
    /// </summary>
    /// <param name="period">The number of data points to average.</param>
    /// <param name="target">The target block that receives computed SMA values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="ActionBlock{Candle}"/> to which candles should be posted in chronological order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static ActionBlock<Candle> SMAActionBlockFactory(
        int period,
        ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<Candle>(period);
        var sum = 0m;

        var block = new ActionBlock<Candle>(
            async candle =>
            {
                sum += candle.Close;
                window.Enqueue(candle);

                if (window.Count > period)
                {
                    sum -= window.Dequeue().Close;
                }

                if (window.Count == period)
                {
                    await target.SendAsync((candle.Timestamp, sum / period), cancellationToken);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });

        // Propagate completion to the target block.
        block.Completion.ContinueWith(
            t => target.Complete(), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

        return block;
    }

    /// <summary>
    /// Creates an <see cref="ActionBlock{Candle}"/> that computes the exponential moving average (EMA)
    /// of closing prices over the specified period and forwards each result to <paramref name="target"/>.
    /// The first value emitted is the SMA of the initial <paramref name="period"/> data points;
    /// subsequent values use the smoothing factor <c>k = 2 / (period + 1)</c>.
    /// </summary>
    /// <param name="period">The number of data points used for the initial SMA and smoothing factor.</param>
    /// <param name="target">The target block that receives computed EMA values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="ActionBlock{Candle}"/> to which candles should be posted in chronological order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static ActionBlock<Candle> EMAActionBlockFactory(
        int period,
        ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var count = 0;
        var sum = 0m;
        var ema = 0m;
        var k = 2m / (period + 1);

        var block = new ActionBlock<Candle>(
            async candle =>
            {
                count++;

                if (count <= period)
                {
                    sum += candle.Close;

                    if (count == period)
                    {
                        ema = sum / period;
                        await target.SendAsync((candle.Timestamp, ema), cancellationToken);
                    }
                }
                else
                {
                    ema = (candle.Close * k) + (ema * (1 - k));
                    await target.SendAsync((candle.Timestamp, ema), cancellationToken);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });

        block.Completion.ContinueWith(
            t => target.Complete(), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

        return block;
    }

    /// <summary>
    /// Creates an <see cref="ActionBlock{Candle}"/> that computes the weighted moving average (WMA)
    /// of closing prices over the specified period and forwards each result to <paramref name="target"/>.
    /// Weights increase linearly: the oldest value in the window receives weight 1, the newest receives
    /// weight equal to <paramref name="period"/>.
    /// </summary>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="target">The target block that receives computed WMA values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="ActionBlock{Candle}"/> to which candles should be posted in chronological order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static ActionBlock<Candle> WMAActionBlockFactory(
        int period,
        ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var window = new Queue<decimal>(period);
        var divisor = period * (period + 1) / 2m;

        var block = new ActionBlock<Candle>(
            async candle =>
            {
                window.Enqueue(candle.Close);

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

                    await target.SendAsync((candle.Timestamp, weightedSum / divisor), cancellationToken);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });

        block.Completion.ContinueWith(
            t => target.Complete(), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

        return block;
    }

    /// <summary>
    /// Creates an <see cref="ActionBlock{Candle}"/> that computes the Hull moving average (HMA)
    /// of closing prices over the specified period and forwards each result to <paramref name="target"/>.
    /// HMA is calculated as <c>WMA(√n)</c> of <c>2 × WMA(n/2) − WMA(n)</c>, which reduces lag
    /// while maintaining smoothness.
    /// </summary>
    /// <param name="period">The number of data points for the full WMA window. Must be at least 2.</param>
    /// <param name="target">The target block that receives computed HMA values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="ActionBlock{Candle}"/> to which candles should be posted in chronological order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 2.</exception>
    public static ActionBlock<Candle> HMAActionBlockFactory(
        int period,
        ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 2);

        var halfPeriod = period / 2;
        var sqrtPeriod = Math.Max(1, (int)Math.Round(Math.Sqrt(period)));

        var priceWindow = new Queue<decimal>(period);
        var diffWindow = new Queue<decimal>(sqrtPeriod);

        var block = new ActionBlock<Candle>(
            async candle =>
            {
                priceWindow.Enqueue(candle.Close);

                if (priceWindow.Count > period)
                {
                    priceWindow.Dequeue();
                }

                if (priceWindow.Count == period)
                {
                    var prices = priceWindow.ToArray();
                    var wmaFull = ComputeWMA(prices, 0, period);
                    var wmaHalf = ComputeWMA(prices, period - halfPeriod, halfPeriod);
                    var diff = (2m * wmaHalf) - wmaFull;

                    diffWindow.Enqueue(diff);

                    if (diffWindow.Count > sqrtPeriod)
                    {
                        diffWindow.Dequeue();
                    }

                    if (diffWindow.Count == sqrtPeriod)
                    {
                        var diffs = diffWindow.ToArray();
                        var hma = ComputeWMA(diffs, 0, sqrtPeriod);
                        await target.SendAsync((candle.Timestamp, hma), cancellationToken);
                    }
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });

        block.Completion.ContinueWith(
            t => target.Complete(), cancellationToken, TaskContinuationOptions.OnlyOnRanToCompletion, TaskScheduler.Default);

        return block;
    }

    private static decimal ComputeWMA(decimal[] values, int start, int count)
    {
        var weightedSum = 0m;

        for (var i = 0; i < count; i++)
        {
            weightedSum += values[start + i] * (i + 1);
        }

        return weightedSum / (count * (count + 1) / 2m);
    }
}
