namespace OpenQuant.Models;

/// <summary>
/// A candle enriched with computed indicator values.
/// Every input candle produces an <see cref="EnrichedCandle"/>; indicators that have not yet
/// accumulated enough data (warm-up period) are absent from the <see cref="Indicators"/> dictionary.
/// </summary>
public sealed record EnrichedCandle
{
    /// <summary>
    /// Gets the original OHLCV candle.
    /// </summary>
    public required Candle Candle { get; init; }

    /// <summary>
    /// Gets the computed indicator values keyed by indicator name.
    /// Indicators that have not emitted a value for this candle's timestamp are absent.
    /// </summary>
    public required IReadOnlyDictionary<string, decimal> Indicators { get; init; }
}
