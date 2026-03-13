using OpenQuant.Models;

namespace OpenQuant;

/// <summary>
/// Defines the contract for a market data provider that retrieves financial candles.
/// </summary>
public interface IMarketDataProvider
{
    /// <summary>
    /// Gets the display name of this data provider.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// Retrieves historical candles for the given symbol within the specified date range.
    /// </summary>
    Task<IReadOnlyList<Candle>> GetHistoricalCandlesAsync(
        string symbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest candle for the given symbol.
    /// </summary>
    Task<Candle?> GetLatestCandleAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
