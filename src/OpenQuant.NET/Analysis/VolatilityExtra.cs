using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Volatility indicators missing from original library.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class VolatilityExtra
{
    /// <summary>
    /// Creates a transform block that computes the Average Day Range.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AverageDayRange(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="AverageDayRange"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AverageDayRangeLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Bollinger Band Width.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> BollingerBandWidth(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="BollingerBandWidth"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int BollingerBandWidthLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Chaikin Volatility.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinVolatility(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ChaikinVolatility"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ChaikinVolatilityLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Mass Index.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MassIndex(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="MassIndex"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int MassIndexLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Percent B.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PercentB(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="PercentB"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int PercentBLookback() => 0;
    #pragma warning restore S3400
}