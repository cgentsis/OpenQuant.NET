using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides moving median calculations as TPL Dataflow blocks.
/// </summary>
public static class MovingMedian
{
    /// <summary>
    /// Creates an <see cref="ActionBlock{Candle}"/> that computes the moving median
    /// of closing prices over the specified period and forwards each result to <paramref name="target"/>.
    /// For an odd period the middle value is returned; for an even period the average of the two
    /// middle values is returned.
    /// </summary>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="target">The target block that receives computed median values.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>An <see cref="ActionBlock{Candle}"/> to which candles should be posted in chronological order.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static ActionBlock<Candle> MedianActionBlockFactory(
        int period,
        ITargetBlock<(DateTimeOffset Timestamp, decimal Value)> target,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var insertionOrder = new Queue<decimal>(period);
        var sorted = new List<decimal>(period);

        var block = new ActionBlock<Candle>(
            async candle =>
            {
                insertionOrder.Enqueue(candle.Close);

                // Binary-search insert to maintain sorted order — O(n) due to list shift,
                // but avoids the O(n log n) full sort that was here before.
                var insertAt = sorted.BinarySearch(candle.Close);
                sorted.Insert(insertAt >= 0 ? insertAt : ~insertAt, candle.Close);

                if (insertionOrder.Count > period)
                {
                    var removed = insertionOrder.Dequeue();
                    var removeAt = sorted.BinarySearch(removed);
                    sorted.RemoveAt(removeAt);
                }

                if (insertionOrder.Count == period)
                {
                    var mid = period / 2;

                    var median = period % 2 == 0
                        ? (sorted[mid - 1] + sorted[mid]) / 2m
                        : sorted[mid];

                    await target.SendAsync((candle.Timestamp, median), cancellationToken);
                }
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });

        _ = DataflowHelpers.PropagateCompletionAsync(block, target);

        return block;
    }
}
