using System;
using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides a generic factory for creating moving average indicator transform blocks.
/// This factory centralises common logic while preserving the Dataflow architecture requirements.
/// </summary>
internal static class IndicatorFactory
{
    /// <summary>
    /// Creates a TransformBlock that computes an indicator value using the provided computation delegate.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="period">The number of data points to use for the indicator calculation.</param>
    /// <param name="computationDelegate">The computation delegate that performs the indicator calculation.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Create(
        string name,
        int period,
        Func<EnrichedCandle, decimal> computationDelegate,
        CancellationToken cancellationToken = default)
    {
        // Check for cancellation early to match the expected behavior
        if (cancellationToken.IsCancellationRequested)
        {
            return new TransformBlock<EnrichedCandle, EnrichedCandle>(
                enriched => enriched,
                new ExecutionDataflowBlockOptions
                {
                    CancellationToken = cancellationToken,
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
                enriched.Indicators[name] = value;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }
}