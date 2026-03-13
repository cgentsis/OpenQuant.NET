namespace OpenQuant.Models;

/// <summary>
/// Represents a single OHLCV price candle for a financial instrument.
/// </summary>
public sealed record Candle
{
    public required DateTimeOffset Timestamp { get; init; }
    public required decimal Open { get; init; }
    public required decimal High { get; init; }
    public required decimal Low { get; init; }
    public required decimal Close { get; init; }
    public required long Volume { get; init; }
}
