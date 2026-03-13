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
}
