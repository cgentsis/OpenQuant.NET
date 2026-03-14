using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides single-bar price transform calculations as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. These indicators are stateless—every
/// candle produces an output value immediately (no warm-up period required), with the exception
/// of <see cref="TrueRange"/> which requires one prior candle.
/// </summary>
public static class PriceTransform
{
    /// <summary>
    /// Creates a transform block that computes the average price: (Open + High + Low + Close) / 4.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AvgPrice(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                enriched.Indicators[name] = (c.Open + c.High + c.Low + c.Close) / 4m;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the median price: (High + Low) / 2.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MedPrice(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                enriched.Indicators[name] = (c.High + c.Low) / 2m;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the typical price: (High + Low + Close) / 3.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TypPrice(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                enriched.Indicators[name] = (c.High + c.Low + c.Close) / 3m;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the weighted close price: (High + Low + 2 × Close) / 4.
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> WclPrice(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                enriched.Indicators[name] = (c.High + c.Low + c.Close + c.Close) / 4m;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the Balance of Power: (Close − Open) / (High − Low).
    /// Returns 0 when High equals Low (zero range).
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Bop(
        string name,
        CancellationToken cancellationToken = default)
    {
        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;
                var range = c.High - c.Low;
                enriched.Indicators[name] = range == 0m ? 0m : (c.Close - c.Open) / range;
                return enriched;
            },
            new ExecutionDataflowBlockOptions
            {
                CancellationToken = cancellationToken,
                MaxDegreeOfParallelism = 1,
            });
    }

    /// <summary>
    /// Creates a transform block that computes the True Range:
    /// <c>max(High − Low, |High − PreviousClose|, |Low − PreviousClose|)</c>.
    /// The first candle has no output (lookback of 1).
    /// </summary>
    /// <param name="name">The key under which the value is stored in <see cref="EnrichedCandle.Indicators"/>.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> TrueRange(
        string name,
        CancellationToken cancellationToken = default)
    {
        decimal? prevClose = null;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var c = enriched.Candle;

                if (prevClose.HasValue)
                {
                    var hl = c.High - c.Low;
                    var hc = Math.Abs(c.High - prevClose.Value);
                    var lc = Math.Abs(c.Low - prevClose.Value);
                    enriched.Indicators[name] = Math.Max(hl, Math.Max(hc, lc));
                }

                prevClose = c.Close;
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

    /// <summary>Returns the number of input bars consumed before <see cref="AvgPrice"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int AvgPriceLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="MedPrice"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int MedPriceLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="TypPrice"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int TypPriceLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="WclPrice"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int WclPriceLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="Bop"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int BopLookback() => 0;

    /// <summary>Returns the number of input bars consumed before <see cref="TrueRange"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    public static int TrueRangeLookback() => 1;
#pragma warning restore S3400
}
