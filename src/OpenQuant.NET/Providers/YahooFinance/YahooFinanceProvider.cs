using System.Net.Http.Json;
using OpenQuant.Models;
using OpenQuant.Providers.YahooFinance.Dto;

namespace OpenQuant.Providers.YahooFinance;

/// <summary>
/// An <see cref="IMarketDataProvider"/> that retrieves OHLCV candle data from Yahoo Finance.
/// Uses the public v8 chart API.
/// </summary>
public sealed class YahooFinanceProvider : IMarketDataProvider
{
    private static readonly Uri DefaultBaseUri = new("https://query1.finance.yahoo.com/v8/finance/chart");

    private readonly HttpClient _httpClient;
    private readonly Uri _baseUri;

    /// <summary>
    /// Initializes a new instance of the <see cref="YahooFinanceProvider"/> class.
    /// The caller is responsible for configuring and disposing the client.
    /// </summary>
    /// <param name="httpClient">The <see cref="HttpClient"/> used to call the Yahoo Finance API.</param>
    /// <param name="baseUri">Optional base URI override for the Yahoo Finance chart API.</param>
    public YahooFinanceProvider(HttpClient httpClient, Uri? baseUri = null)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        _baseUri = baseUri ?? DefaultBaseUri;
    }

    /// <summary>
    /// Gets the display name of this data provider. Always returns <c>"Yahoo Finance"</c>.
    /// </summary>
    public string Name => "Yahoo Finance";

    /// <summary>
    /// Retrieves historical daily candles for the given symbol from Yahoo Finance
    /// within the specified date range using the v8 chart API.
    /// </summary>
    /// <param name="symbol">The ticker symbol (e.g. <c>"AAPL"</c>).</param>
    /// <param name="from">Start of the date range (inclusive).</param>
    /// <param name="to">End of the date range (inclusive).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of <see cref="Candle"/> objects ordered by timestamp.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="symbol"/> is null or whitespace.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error or null response.</exception>
    public async Task<IReadOnlyList<Candle>> GetHistoricalCandlesAsync(
        string symbol,
        DateTimeOffset from,
        DateTimeOffset to,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var period1 = from.ToUnixTimeSeconds();
        var period2 = to.ToUnixTimeSeconds();
        var url = $"{_baseUri}/{Uri.EscapeDataString(symbol)}?period1={period1}&period2={period2}&interval=1d&events=history";

        var response = await _httpClient.GetFromJsonAsync<YahooChartResponse>(url, cancellationToken)
            ?? throw new HttpRequestException("Received null response from Yahoo Finance.");

        return ParseCandles(response);
    }

    /// <summary>
    /// Retrieves the most recent <paramref name="count"/> trading-day candles up to and including
    /// the specified <paramref name="asOf"/> date from Yahoo Finance. Fetches extra calendar days
    /// to account for weekends and holidays, then trims to the requested count.
    /// </summary>
    /// <param name="symbol">The ticker symbol (e.g. <c>"AAPL"</c>).</param>
    /// <param name="asOf">The reference date (inclusive upper bound).</param>
    /// <param name="count">The number of trading-day candles to retrieve.</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>A read-only list of up to <paramref name="count"/> candles ordered by timestamp.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="symbol"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="count"/> is less than 1.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error or null response.</exception>
    public async Task<IReadOnlyList<Candle>> GetCandlesAsync(
        string symbol,
        DateTimeOffset asOf,
        int count,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        ArgumentOutOfRangeException.ThrowIfLessThan(count, 1);

        // Fetch ~1.5× calendar days to account for weekends/holidays (≈5 trading days per 7 calendar days).
        var calendarDays = (int)Math.Ceiling(count * 1.5) + 10;
        var from = asOf.AddDays(-calendarDays);

        var candles = await GetHistoricalCandlesAsync(symbol, from, asOf, cancellationToken);

        if (candles.Count <= count)
        {
            return candles;
        }

        return candles.Skip(candles.Count - count).ToList().AsReadOnly();
    }

    /// <summary>
    /// Retrieves the most recent daily candle for the given symbol from Yahoo Finance
    /// using the v8 chart API with a <c>1d</c> range.
    /// </summary>
    /// <param name="symbol">The ticker symbol (e.g. <c>"MSFT"</c>).</param>
    /// <param name="cancellationToken">Optional cancellation token.</param>
    /// <returns>The latest <see cref="Candle"/>, or <see langword="null"/> if no data is available.</returns>
    /// <exception cref="ArgumentException">Thrown when <paramref name="symbol"/> is null or whitespace.</exception>
    /// <exception cref="HttpRequestException">Thrown when the API returns an error or null response.</exception>
    public async Task<Candle?> GetLatestCandleAsync(
        string symbol,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);

        var url = $"{_baseUri}/{Uri.EscapeDataString(symbol)}?range=1d&interval=1d&events=history";

        var response = await _httpClient.GetFromJsonAsync<YahooChartResponse>(url, cancellationToken)
            ?? throw new HttpRequestException("Received null response from Yahoo Finance.");

        var candles = ParseCandles(response);
        return candles.Count > 0 ? candles[^1] : null;
    }

    private static List<Candle> ParseCandles(YahooChartResponse response)
    {
        var chart = response.Chart;

        if (chart.Error is { } error)
        {
            throw new HttpRequestException($"Yahoo Finance error: {error.Code} – {error.Description}");
        }

        var result = chart.Result?.FirstOrDefault();
        var timestamps = result?.Timestamp;
        var quote = result?.Indicators.Quote?.FirstOrDefault();

        if (timestamps is null || quote is null)
        {
            return [];
        }

        var candles = new List<Candle>(timestamps.Count);

        for (var i = 0; i < timestamps.Count; i++)
        {
            var open = quote.Open?.ElementAtOrDefault(i);
            var high = quote.High?.ElementAtOrDefault(i);
            var low = quote.Low?.ElementAtOrDefault(i);
            var close = quote.Close?.ElementAtOrDefault(i);
            var volume = quote.Volume?.ElementAtOrDefault(i);

            // Skip entries with missing data.
            if (open is null || high is null || low is null || close is null || volume is null)
            {
                continue;
            }

            candles.Add(new Candle
            {
                Timestamp = DateTimeOffset.FromUnixTimeSeconds(timestamps[i]),
                Open = open.Value,
                High = high.Value,
                Low = low.Value,
                Close = close.Value,
                Volume = volume.Value,
            });
        }

        return candles;
    }
}
