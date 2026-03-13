using System.Net;
using System.Text;
using OpenQuant.Providers.YahooFinance;

namespace OpenQuant.Tests.Providers.YahooFinance;

public class YahooFinanceProviderTests
{
    private const string ValidChartJson = """
        {
          "chart": {
            "result": [{
              "timestamp": [1704067200, 1704153600],
              "indicators": {
                "quote": [{
                  "open":   [100.0, 102.0],
                  "high":   [105.0, 107.0],
                  "low":    [99.0,  101.0],
                  "close":  [104.0, 106.0],
                  "volume": [1000,  2000]
                }]
              }
            }],
            "error": null
          }
        }
        """;

    private const string EmptyResultJson = """
        {
          "chart": {
            "result": [{
              "timestamp": null,
              "indicators": { "quote": [{}] }
            }],
            "error": null
          }
        }
        """;

    private const string ErrorJson = """
        {
          "chart": {
            "result": null,
            "error": { "code": "Not Found", "description": "No data found" }
          }
        }
        """;

    [Fact]
    public async Task GetHistoricalCandlesAsync_ParsesValidResponse()
    {
        using var client = CreateMockClient(ValidChartJson);
        var provider = new YahooFinanceProvider(client);

        var candles = await provider.GetHistoricalCandlesAsync(
            "AAPL",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UtcNow);

        Assert.Equal(2, candles.Count);
        Assert.Equal(104.0m, candles[0].Close);
        Assert.Equal(106.0m, candles[1].Close);
        Assert.Equal(1000L, candles[0].Volume);
        Assert.Equal(2000L, candles[1].Volume);
    }

    [Fact]
    public async Task GetHistoricalCandlesAsync_ReturnsEmptyForNoData()
    {
        using var client = CreateMockClient(EmptyResultJson);
        var provider = new YahooFinanceProvider(client);

        var candles = await provider.GetHistoricalCandlesAsync(
            "INVALID",
            DateTimeOffset.UnixEpoch,
            DateTimeOffset.UtcNow);

        Assert.Empty(candles);
    }

    [Fact]
    public async Task GetHistoricalCandlesAsync_ThrowsOnErrorResponse()
    {
        using var client = CreateMockClient(ErrorJson);
        var provider = new YahooFinanceProvider(client);

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            provider.GetHistoricalCandlesAsync("BAD", DateTimeOffset.UnixEpoch, DateTimeOffset.UtcNow));
    }

    [Fact]
    public async Task GetLatestCandleAsync_ReturnsLastCandle()
    {
        using var client = CreateMockClient(ValidChartJson);
        var provider = new YahooFinanceProvider(client);

        var candle = await provider.GetLatestCandleAsync("AAPL");

        Assert.NotNull(candle);
        Assert.Equal(106.0m, candle.Close);
    }

    [Fact]
    public void Name_ReturnsYahooFinance()
    {
        using var client = new HttpClient();
        var provider = new YahooFinanceProvider(client);

        Assert.Equal("Yahoo Finance", provider.Name);
    }

    private static HttpClient CreateMockClient(string json, HttpStatusCode status = HttpStatusCode.OK)
    {
        var handler = new FakeHttpMessageHandler(json, status);
        return new HttpClient(handler) { BaseAddress = new Uri("https://localhost") };
    }

    /// <summary>
    /// A minimal <see cref="HttpMessageHandler"/> that returns a canned JSON response.
    /// </summary>
    private sealed class FakeHttpMessageHandler(string json, HttpStatusCode statusCode) : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(statusCode)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json"),
            };
            return Task.FromResult(response);
        }
    }
}
