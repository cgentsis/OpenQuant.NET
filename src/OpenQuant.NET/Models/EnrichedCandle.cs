namespace OpenQuant.Models;

/// <summary>
/// A candle enriched with computed indicator values as it flows through a pipeline.
/// Every input candle produces an <see cref="EnrichedCandle"/>; indicators that have not yet
/// accumulated enough data (warm-up period) are absent from the <see cref="Indicators"/> dictionary.
/// </summary>
public sealed class EnrichedCandle
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EnrichedCandle"/> class wrapping the specified candle.
    /// </summary>
    /// <param name="candle">The original OHLCV candle.</param>
    public EnrichedCandle(Candle candle)
    {
        Candle = candle;
    }

    /// <summary>Gets the original OHLCV candle.</summary>
    public Candle Candle { get; }

    /// <summary>
    /// Gets the computed indicator values keyed by indicator name.
    /// Each pipeline stage adds its value here; indicators still in warm-up are absent.
    /// </summary>
    public Dictionary<string, decimal> Indicators { get; } = [];
}
