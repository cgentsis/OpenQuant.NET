using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Provides arithmetic operations over candle prices as TPL Dataflow
/// <see cref="TransformBlock{TInput,TOutput}"/> stages. Results are stored in
/// <see cref="EnrichedCandle.Indicators"/> under the supplied name, which defaults to the method name.
/// </summary>
public static class MathOperators
{
    /// <summary>
    /// Creates a transform block that computes the sum of the high and low prices.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Add(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(Add);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                enriched.Indicators[key] = enriched.Candle.High + enriched.Candle.Low;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Add"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the operation is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int AddLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the difference between the high and low prices.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sub(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(Sub);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                enriched.Indicators[key] = enriched.Candle.High - enriched.Candle.Low;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Sub"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the operation is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int SubLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the product of the high and low prices.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Mult(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(Mult);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                enriched.Indicators[key] = enriched.Candle.High * enriched.Candle.Low;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Mult"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the operation is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int MultLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that divides the high price by the low price.
    /// No value is stored when the low price is zero.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Div(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(Div);

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                var low = enriched.Candle.Low;
                if (low != 0m)
                {
                    enriched.Indicators[key] = enriched.Candle.High / low;
                }

                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="Div"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the operation is stateless.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int DivLookback(int period = 14) => 0;
#pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes a running cumulative sum of closing prices
    /// across the entire stream. The period is accepted for API consistency but does not limit the sum.
    /// </summary>
    /// <param name="period">The period, accepted for API consistency and validated to be positive.</param>
    /// <param name="name">The indicator key. Defaults to the method name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="period"/> is less than 1.</exception>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CumSum(
        int period,
        string? name = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentOutOfRangeException.ThrowIfLessThan(period, 1);
        var key = name ?? nameof(CumSum);
        var total = 0m;

        return new TransformBlock<EnrichedCandle, EnrichedCandle>(
            enriched =>
            {
                total += enriched.Candle.Close;
                enriched.Indicators[key] = total;
                return enriched;
            },
            CreateOptions(cancellationToken));
    }

    /// <summary>Returns the number of input bars consumed before <see cref="CumSum"/> produces its first value.</summary>
    /// <param name="period">The period, ignored because the cumulative sum starts immediately.</param>
    /// <returns>Always zero.</returns>
#pragma warning disable S3400
    public static int CumSumLookback(int period = 14) => 0;
#pragma warning restore S3400

    private static ExecutionDataflowBlockOptions CreateOptions(CancellationToken cancellationToken) =>
        new()
        {
            CancellationToken = cancellationToken,
            MaxDegreeOfParallelism = 1,
        };
}
