namespace OpenQuant.Models;

/// <summary>
/// Represents a single price quote for a financial instrument.
/// </summary>
public sealed record Quote
{
    public required string Symbol { get; init; }
    public required DateTimeOffset Timestamp { get; init; }
    public required decimal Open { get; init; }
    public required decimal High { get; init; }
    public required decimal Low { get; init; }
    public required decimal Close { get; init; }
    public required long Volume { get; init; }
}
