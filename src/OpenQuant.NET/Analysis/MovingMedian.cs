using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides moving median calculations as a TPL Dataflow <see cref="TransformBlock{TInput,TOutput}"/> stage.
/// </summary>
public static class MovingMedian
{
    /// <summary>
    /// Creates a <see cref="TransformBlock{EnrichedCandle, EnrichedCandle}"/> that computes the
    /// moving median of closing prices over the specified period.
    /// For an odd period the middle value is returned; for an even period the average of the two
    /// middle values is returned.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points in the window.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when period is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Median(
        string name,
        int period,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);

        var insertionOrder = new Queue<decimal>(period);
        var sorted = new List<decimal>(period);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                insertionOrder.Enqueue(enriched.Candle.Close);

                var insertAt = sorted.BinarySearch(enriched.Candle.Close);
                sorted.Insert(insertAt >= 0 ? insertAt : ~insertAt, enriched.Candle.Close);

                if (insertionOrder.Count > period)
                {
                    var removed = insertionOrder.Dequeue();
                    var removeAt = sorted.BinarySearch(removed);
                    sorted.RemoveAt(removeAt);
                }

                if (insertionOrder.Count == period)
                {
                    var mid = period / 2;

                    enriched.Indicators[name] = period % 2 == 0
                        ? (sorted[mid - 1] + sorted[mid]) / 2m
                        : sorted[mid];
                }

                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }
}
