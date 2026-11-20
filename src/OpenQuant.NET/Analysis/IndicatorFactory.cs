using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides a generic factory for creating moving average indicator transform blocks.
/// This factory centralises common logic while preserving the Dataflow architecture requirements.
/// </summary>
public static class IndicatorFactory
{
    /// <summary>
    /// Creates a TransformBlock that computes an indicator value using the provided computation delegate.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="computationDelegate">The computation delegate; return <see langword="null"/> while the indicator is warming up.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Create(
        string name,
        Func<EnrichedCandle, decimal?> computationDelegate,
        CancellationToken cancellationToken = default)
    {
        // Check for cancellation early to match the expected behavior
        if (cancellationToken.IsCancellationRequested)
        {
            // Do not pass the cancelled token to the block itself: a cancelled Dataflow block
            // rejects messages, while this pass-through stage must let candles continue unchanged.
            return new TransformBlock<EnrichedCandle, EnrichedCandle>(
                enriched => enriched,
                new ExecutionDataflowBlockOptions
                {
                    MaxDegreeOfParallelism = 1,
                });
        }

        // Return a proper block with the computation delegate - this preserves all Dataflow patterns
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return enriched;
                }

                var value = computationDelegate(enriched);
                if (value.HasValue)
                {
                    enriched.Indicators[name] = value.Value;
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
