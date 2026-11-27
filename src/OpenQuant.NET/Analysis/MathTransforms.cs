using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Math Transform functions missing from the original library.
/// Each returns a TransformBlock that forwards the input candle unchanged.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class MathTransforms
{
    /// <summary>
    /// Creates a transform block that computes the arc cosine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Acos(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Acos"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AcosLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the arc sine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Asin(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Asin"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AsinLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the arc tangent of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Atan(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Atan"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AtanLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the ceiling of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ceil(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Ceil"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CeilLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the cosine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cos(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Cos"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CosLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the hyperbolic cosine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Cosh(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Cosh"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CoshLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the exponential of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Exp(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Exp"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ExpLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the floor of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Floor(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Floor"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int FloorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the natural logarithm of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Ln(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Ln"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int LnLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the base-10 logarithm of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Log10(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Log10"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int Log10Lookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the sine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sin(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Sin"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int SinLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the hyperbolic sine of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sinh(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Sinh"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int SinhLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the square root of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Sqrt(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Sqrt"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int SqrtLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the tangent of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tan(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Tan"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int TanLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the hyperbolic tangent of the close price.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Tanh(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Tanh"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int TanhLookback() => 0;
    #pragma warning restore S3400
}