namespace OpenQuant.Models;

/// <summary>
/// Represents a single OHLCV price candle for a financial instrument.
/// </summary>
public sealed record Candle
{
    /// <summary>Gets the date and time of the candle.</summary>
    public required DateTimeOffset Timestamp { get; init; }

    /// <summary>Gets the opening price.</summary>
    public required decimal Open { get; init; }

    /// <summary>Gets the highest price.</summary>
    public required decimal High { get; init; }

    /// <summary>Gets the lowest price.</summary>
    public required decimal Low { get; init; }

    /// <summary>Gets the closing price.</summary>
    public required decimal Close { get; init; }

    /// <summary>Gets the trading volume.</summary>
    public required long Volume { get; init; }
}
