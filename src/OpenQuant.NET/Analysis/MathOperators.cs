using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Math Operators (ADD, DIV, MULT, SUB, CUMSUM) missing from original library.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class MathOperators
{
    /// <summary>
    /// Creates a transform block that computes the sum of two prices.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Add(string name, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Add"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AddLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the division of two prices.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Div(string name, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Div"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int DivLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the multiplication of two prices.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Mult(string name, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Mult"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int MultLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the subtraction of two prices.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sub(string name, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Sub"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int SubLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the cumulative sum.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CumSum(string name, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="CumSum"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CumSumLookback() => 0;
    #pragma warning restore S3400
}