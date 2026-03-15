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
    /// Retrieves the most recent <paramref name="count"/> trading-day candles up to and including
    /// the specified <paramref name="asOf"/> date. The provider fetches enough calendar days to
    /// satisfy the requested trading-day count and returns exactly <paramref name="count"/> candles
    /// (or fewer if insufficient history is available).
    /// </summary>
    /// <param name="symbol">The ticker symbol.</param>
    /// <param name="asOf">The reference date (inclusive upper bound).</param>
    /// <param name="count">The number of trading-day candles to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of up to <paramref name="count"/> candles ordered by timestamp.</returns>
    Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string symbol,
        DateTimeOffset asOf,
        int count,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the latest candle for the given symbol.
    /// </summary>
    Task<Candle?> GetLatestCandleAsync(
        string symbol,
        CancellationToken cancellationToken = default);
}
