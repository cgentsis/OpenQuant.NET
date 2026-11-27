using System.Threading.Tasks.Dataflow;
using OpenQuant.Models;

namespace OpenQuant.Analysis;

/// <summary>
/// Additional Overlap Study indicators missing from original library.
/// Placeholder implementations – replace with real calculations.
/// </summary>
public static class OverlapStudiesExtra
{
    /// <summary>
    /// Creates a transform block that computes Acceleration Bands.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> AccelerationBands(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="AccelerationBands"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int AccelerationBandsLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Arnaud Legoux Moving Average.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ArnaudLegouxMA(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ArnaudLegouxMA"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ArnaudLegouxMALookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Chande Kroll Stop.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> ChandeKrollStop(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="ChandeKrollStop"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ChandeKrollStopLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Donchian Channels.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> DonchianChannels(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="DonchianChannels"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int DonchianChannelsLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Fractal Adaptive Moving Average.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> FractalAdaptiveMA(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="FractalAdaptiveMA"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int FractalAdaptiveMALookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Ichimoku Kinko Hyo.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> IchimokuKinkoHyo(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="IchimokuKinkoHyo"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int IchimokuKinkoHyoLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Keltner Channels.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> KeltnerChannels(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="KeltnerChannels"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int KeltnerChannelsLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes MESA Adaptive Moving Average.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> MESAAdaptiveMA(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="MESAAdaptiveMA"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int MESAAdaptiveMALookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes McGinley Dynamic Moving Average.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> McGinleyDynamic(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="McGinleyDynamic"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int McGinleyDynamicLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Volume Weighted Moving Average.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Vwma(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Vwma"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int VwmaLookback() => 0;
    #pragma warning restore S3400

    /// <summary>
    /// Creates a transform block that computes Zigzag indicator.
    /// </summary>
    /// <param name="period">The period for computation (placeholder - no effect in identity pass-through).</param>
    /// <returns>A transform block to use as a pipeline stage.</returns>
    public static TransformBlock<EnrichedCandle, EnrichedCandle> Zigzag(int period) =>
        new TransformBlock<EnrichedCandle, EnrichedCandle>(c => c);

    /// <summary>Returns the number of input bars consumed before <see cref="Zigzag"/> produces its first value.</summary>
    /// <returns>The lookback count.</returns>
    #pragma warning disable S3400
    public static int ZigzagLookback() => 0;
    #pragma warning restore S3400
}