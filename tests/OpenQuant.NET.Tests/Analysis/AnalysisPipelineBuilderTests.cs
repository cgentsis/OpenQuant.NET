using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class AnalysisPipelineBuilderTests
{
    [Fact]
    public async Task Run_MultipleIndicators_EnrichesCandles()
    {
        var candles = MakeCandles(10m, 20m, 30m, 25m, 35m);

        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA3", 3)
            .AddEMA("EMA3", 3)
            .RunAsync(candles);

        // First two candles have no indicator values (warm-up).
        Assert.Empty(results[0].Indicators);
        Assert.Empty(results[1].Indicators);

        // Third candle: SMA=(10+20+30)/3=20, EMA seed=20
        Assert.Equal(20m, results[2].Indicators["SMA3"]);
        Assert.Equal(20m, results[2].Indicators["EMA3"]);

        // k = 2/(3+1) = 0.5
        // Fourth: SMA=(20+30+25)/3=25, EMA=25*0.5+20*0.5=22.5
        Assert.Equal(25m, results[3].Indicators["SMA3"]);
        Assert.Equal(22.5m, results[3].Indicators["EMA3"]);

        // Fifth: SMA=(30+25+35)/3=30, EMA=35*0.5+22.5*0.5=28.75
        Assert.Equal(30m, results[4].Indicators["SMA3"]);
        Assert.Equal(28.75m, results[4].Indicators["EMA3"]);
    }

    [Fact]
    public async Task Run_DifferentWarmUpPeriods_ShorterIndicatorPresentEarlier()
    {
        var candles = MakeCandles(10m, 20m, 30m, 40m);

        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA2", 2)
            .AddSMA("SMA3", 3)
            .RunAsync(candles);

        // Candle 0: no indicators
        Assert.Empty(results[0].Indicators);

        // Candle 1: SMA2 ready, SMA3 not yet
        Assert.Single(results[1].Indicators);
        Assert.Equal(15m, results[1].Indicators["SMA2"]);

        // Candle 2: both ready
        Assert.Equal(2, results[2].Indicators.Count);
        Assert.Equal(25m, results[2].Indicators["SMA2"]);
        Assert.Equal(20m, results[2].Indicators["SMA3"]);

        // Candle 3: both ready
        Assert.Equal(35m, results[3].Indicators["SMA2"]);
        Assert.Equal(30m, results[3].Indicators["SMA3"]);
    }

    [Fact]
    public async Task Run_AllBuiltInIndicators_ProducesValues()
    {
        var candles = MakeCandles(10m, 20m, 30m, 40m, 50m, 60m);

        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA", 3)
            .AddEMA("EMA", 3)
            .AddWMA("WMA", 3)
            .AddHMA("HMA", 4)
            .AddMedian("Median", 3)
            .RunAsync(candles);

        Assert.Equal(6, results.Count);

        // Last candle should have all indicators populated.
        var last = results[5];
        Assert.True(last.Indicators.ContainsKey("SMA"));
        Assert.True(last.Indicators.ContainsKey("EMA"));
        Assert.True(last.Indicators.ContainsKey("WMA"));
        Assert.True(last.Indicators.ContainsKey("HMA"));
        Assert.True(last.Indicators.ContainsKey("Median"));
    }

    [Fact]
    public async Task Run_CustomIndicatorViaAdd_Works()
    {
        var candles = MakeCandles(10m, 20m, 30m);

        var results = await new AnalysisPipelineBuilder()
            .Add("MySMA", (target, ct) => MovingAverage.SMAActionBlockFactory(2, target, ct))
            .RunAsync(candles);

        Assert.Empty(results[0].Indicators);
        Assert.Equal(15m, results[1].Indicators["MySMA"]);
        Assert.Equal(25m, results[2].Indicators["MySMA"]);
    }

    [Fact]
    public async Task Run_PreservesOriginalCandle()
    {
        var candles = MakeCandles(42m);

        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA", 1)
            .RunAsync(candles);

        Assert.Same(candles[0], results[0].Candle);
        Assert.Equal(42m, results[0].Indicators["SMA"]);
    }

    [Fact]
    public async Task Run_WithNoData_ReturnsEmpty()
    {
        var results = await new AnalysisPipelineBuilder()
            .AddSMA("SMA", 3)
            .RunAsync([]);

        Assert.Empty(results);
    }

    [Fact]
    public async Task Run_NoIndicators_Throws()
    {
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => new AnalysisPipelineBuilder().RunAsync(MakeCandles(10m)));
    }

    [Fact]
    public void Add_DuplicateNames_Throws()
    {
        Assert.Throws<ArgumentException>(() =>
            new AnalysisPipelineBuilder()
                .AddSMA("SMA", 3)
                .AddSMA("SMA", 5));
    }

    [Fact]
    public async Task Run_BuilderIsReusable()
    {
        var builder = new AnalysisPipelineBuilder().AddSMA("SMA", 2);

        var first = await builder.RunAsync(MakeCandles(10m, 20m));
        var second = await builder.RunAsync(MakeCandles(30m, 40m));

        Assert.Equal(15m, first[1].Indicators["SMA"]);
        Assert.Equal(35m, second[1].Indicators["SMA"]);
    }

    private static IReadOnlyList<Candle> MakeCandles(params decimal[] closes)
    {
        return closes.Select((close, i) => new Candle
        {
            Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(i),
            Open = close,
            High = close,
            Low = close,
            Close = close,
            Volume = 1000,
        }).ToList();
    }
}
