using OpenQuant.Models;

namespace OpenQuant;

/// <summary>
/// Defines the contract for a market data provider that retrieves financial quotes.
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>
    /// Gets the display name of this data provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves historical quotes for the given symbol within the specified date range.
    /// </summary>
    Task<IReadOnlyList<Quote>> GetHistoricalQuotesAsync(
        string symbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest quote for the given symbol.
    /// </summary>
    Task<Quote?> GetLatestQuoteAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
