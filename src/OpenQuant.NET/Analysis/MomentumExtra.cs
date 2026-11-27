using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Momentum indicators missing from original library.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class MomentumExtra
{
    /// <summary>
    /// Creates a transform block that computes the Accelerator Oscillator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AcceleratorOscillator(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="AcceleratorOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AcceleratorOscillatorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Awesome Oscillator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AwesomeOscillator(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="AwesomeOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AwesomeOscillatorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Center of Gravity.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CenterOfGravity(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="CenterOfGravity"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CenterOfGravityLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Choppiness Index.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndex(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ChoppinessIndex"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ChoppinessIndexLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Choppiness Index with True Range Box.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChoppinessIndexTrueRangeBox(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ChoppinessIndexTrueRangeBox"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ChoppinessIndexTrueRangeBoxLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Coppock Curve.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CoppockCurve(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="CoppockCurve"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CoppockCurveLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Connors RSI.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ConnorsRSI(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ConnorsRSI"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ConnorsRSILookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Correlation Trend Indicator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> CorrelationTrendIndicator(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="CorrelationTrendIndicator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int CorrelationTrendIndicatorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Derivative Oscillator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DerivativeOscillator(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="DerivativeOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int DerivativeOscillatorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Detrended Price Oscillator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DetrendedPriceOscillator(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="DetrendedPriceOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int DetrendedPriceOscillatorLookback() => 0;
    #pragma warning restore S3400
}