using OpenQuant.Providers.YahooFinance;

namespace OpenQuant.Tests.Providers.YahooFinance.Integration;

[CollectionDefinition("YahooFinance")]
public class YahooFinanceCollection : ICollectionFixture<YahooFinanceFixture>
{
}

public sealed class YahooFinanceFixture : IDisposable
{
    public YahooFinanceFixture()
    {
        HttpClient = new HttpClient();
        HttpClient.DefaultRequestHeaders.UserAgent.ParseAdd("OpenQuant.NET/0.1.0");
        Provider = new YahooFinanceProvider(HttpClient);
    }

    public HttpClient HttpClient { get; }

    public YahooFinanceProvider Provider { get; }

    public void Dispose()
    {
        HttpClient.Dispose();
    }
}

[Trait("Category", "Integration")]
[Collection("YahooFinance")]
public class YahooFinanceProviderIntegrationTests
{
    private readonly YahooFinanceProvider _provider;

    public YahooFinanceProviderIntegrationTests(YahooFinanceFixture fixture)
    {
        _provider = fixture.Provider;
    }

    [Fact]
    public async Task GetHistoricalCandlesAsync_ReturnsValidChronologicalCandles()
    {
        var from = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero);

        var candles = await _provider.GetHistoricalCandlesAsync("AAPL", from, to);

        // Should have at least 15 trading days in January 2025
        Assert.True(candles.Count >= 15, $"Expected at least 15 trading days but got {candles.Count}.");

        // Validate OHLCV data integrity
        Assert.All(candles, c =>
        {
            Assert.True(c.Timestamp >= from, "Candle timestamp should be on or after the start date.");
            Assert.True(c.Open > 0, "Open price should be positive.");
            Assert.True(c.High >= c.Low, "High should be >= Low.");
            Assert.True(c.Close > 0, "Close price should be positive.");
            Assert.True(c.Volume >= 0, "Volume should be non-negative.");
        });

        // Validate chronological order
        for (var i = 1; i < candles.Count; i++)
        {
            Assert.True(candles[i].Timestamp > candles[i - 1].Timestamp, "Candles should be in ascending chronological order.");
        }
    }

    [Fact]
    public async Task GetLatestCandleAsync_ReturnsValidCandle()
    {
        var candle = await _provider.GetLatestCandleAsync("AAPL");

        Assert.NotNull(candle);
        Assert.True(candle.Open > 0, "Open price should be positive.");
        Assert.True(candle.High >= candle.Low, "High should be >= Low.");
        Assert.True(candle.Close > 0, "Close price should be positive.");
        Assert.True(candle.Volume >= 0, "Volume should be non-negative.");
    }

    [Fact]
    public async Task GetHistoricalCandlesAsync_ThrowsForInvalidSymbol()
    {
        var from = new DateTimeOffset(2025, 1, 2, 0, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2025, 1, 31, 0, 0, 0, TimeSpan.Zero);

        await Assert.ThrowsAnyAsync<HttpRequestException>(() =>
            _provider.GetHistoricalCandlesAsync("ZZZNOTREAL123", from, to));
    }
}
