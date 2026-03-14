using OpenQuant.Analysis;
using OpenQuant.Models;
using OpenQuant.Providers.YahooFinance;
using OpenQuant.Tests.Providers.YahooFinance.Integration;
using Xunit.Abstractions;

namespace OpenQuant.Tests.Analysis.Integration;

[Trait("Category", "Integration")]
[Collection("YahooFinance")]
public class AnalysisPipelineIntegrationTests
{
    private readonly YahooFinanceProvider _provider;
    private readonly ITestOutputHelper _output;

    public AnalysisPipelineIntegrationTests(
        YahooFinanceFixture fixture,
        ITestOutputHelper output)
    {
        _provider = fixture.Provider;
        _output = output;
    }

    [Fact]
    public async Task Pipeline_WithRealMSFTData_EnrichesAllCandles()
    {
        // Fetch last ~2 months of MSFT daily candles.
        var to = DateTimeOffset.UtcNow;
        var from = to.AddMonths(-2);

        var candles = await _provider.GetHistoricalCandlesAsync("MSFT", from, to);

        Assert.True(candles.Count >= 30, $"Expected at least 30 trading days but got {candles.Count}.");

        // Build pipeline with every indicator type.
        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA10", 10)
            .AddEMA("EMA10", 10)
            .AddWMA("WMA10", 10)
            .AddHMA("HMA16", 16)
            .AddMedian("Median5", 5)
            .RunAsync(candles);

        // Same number of enriched candles as input.
        Assert.Equal(candles.Count, results.Count);

        // Print header.
        _output.WriteLine(
            $"{"Date",-12} {"Close",10} {"SMA10",10} {"EMA10",10} {"WMA10",10} {"HMA16",10} {"Median5",10}");
        _output.WriteLine(new string('-', 74));

        foreach (var e in results)
        {
            var c = e.Candle;
            _output.WriteLine(
                $"{c.Timestamp:yyyy-MM-dd}  " +
                $"{c.Close,10:F2} " +
                $"{Fmt(e, "SMA10"),10} " +
                $"{Fmt(e, "EMA10"),10} " +
                $"{Fmt(e, "WMA10"),10} " +
                $"{Fmt(e, "HMA16"),10} " +
                $"{Fmt(e, "Median5"),10}");
        }

        // Validate: early candles should have no/partial indicators (warm-up).
        Assert.Empty(results[0].Indicators);

        // Validate: once all indicators are active, every value must be present and positive.
        var allIndicators = new[] { "SMA10", "EMA10", "WMA10", "HMA16", "Median5" };
        var fullyPopulated = results.Where(r => r.Indicators.Count == allIndicators.Length).ToList();

        Assert.True(fullyPopulated.Count > 0, "Expected at least one fully-populated enriched candle.");

        Assert.All(fullyPopulated, r =>
        {
            foreach (var name in allIndicators)
            {
                Assert.True(r.Indicators.ContainsKey(name), $"Missing indicator '{name}'.");
                Assert.True(r.Indicators[name] > 0, $"Indicator '{name}' should be positive.");
            }
        });

        // Validate: original candle reference is preserved.
        for (var i = 0; i < candles.Count; i++)
        {
            Assert.Same(candles[i], results[i].Candle);
        }

        // Validate: indicator values are in a reasonable range relative to closing price.
        foreach (var r in fullyPopulated)
        {
            var close = r.Candle.Close;

            foreach (var name in allIndicators)
            {
                var value = r.Indicators[name];
                var ratio = value / close;

                Assert.True(
                    ratio is > 0.5m and < 2.0m,
                    $"{name}={value:F2} is unreasonably far from Close={close:F2} (ratio={ratio:F4}).");
            }
        }

        _output.WriteLine($"\nTotal candles: {results.Count}, fully enriched: {fullyPopulated.Count}");
    }

    private static string Fmt(EnrichedCandle e, string name)
        => e.Indicators.TryGetValue(name, out var v) ? v.ToString("F2") : "-";
}
