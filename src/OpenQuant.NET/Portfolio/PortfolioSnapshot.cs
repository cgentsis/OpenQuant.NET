namespace OpenQuant.Portfolio;

/// <summary>
/// A read-only snapshot of the portfolio state at a point in time.
/// </summary>
public sealed record PortfolioSnapshot
{
    /// <summary>Gets the total equity converted to the base currency.</summary>
    public required decimal TotalEquity { get; init; }

    /// <summary>Gets the cash balances keyed by currency code.</summary>
    public required IReadOnlyDictionary<string, decimal> CashBalances { get; init; }

    /// <summary>Gets the position market values keyed by instrument symbol.</summary>
    public required IReadOnlyDictionary<string, decimal> PositionValues { get; init; }

    /// <summary>Gets the timestamp when the snapshot was taken.</summary>
    public DateTimeOffset Timestamp { get; init; } = DateTimeOffset.UtcNow;
}
