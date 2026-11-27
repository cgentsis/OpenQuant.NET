using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Volume indicators missing from original library.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class VolumeExtra
{
    /// <summary>
    /// Creates a transform block that computes the Chaikin Money Flow.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChaikinMoneyFlow(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ChaikinMoneyFlow"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ChaikinMoneyFlowLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Elder Force Index.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ElderForceIndex(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ElderForceIndex"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ElderForceIndexLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Arms Ease of Movement.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ArmsEaseOfMovement(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ArmsEaseOfMovement"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ArmsEaseOfMovementLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Klinger Volume Oscillator.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KlingerVolumeOscillator(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="KlingerVolumeOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int KlingerVolumeOscillatorLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Market Facilitation Index.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MarketFacilitationIndex(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="MarketFacilitationIndex"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int MarketFacilitationIndexLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Positive Volume Index.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeIndex(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="PositiveVolumeIndex"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int PositiveVolumeIndexLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes the Positive Volume Oscillator.
    /// </summary>
    /// <param name="name">The name to use for output keys.</param>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> PositiveVolumeOscillator(string name, int period, CancellationToken cancellationToken = default) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="PositiveVolumeOscillator"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int PositiveVolumeOscillatorLookback() => 0;
    #pragma warning restore S3400
}