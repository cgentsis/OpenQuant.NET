using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class VolatilityIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Atr_WithValidPeriod_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var atr = VolatilityIndicators.Atr("ATR", 3);
        var sink = CreateSink(results);
        atr.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 12m, 8m, 10m, dayOffset: 0),
            MakeCandle(10m, 14m, 9m, 13m, dayOffset: 1),
            MakeCandle(13m, 16m, 11m, 15m, dayOffset: 2),
            MakeCandle(15m, 15m, 10m, 12m, dayOffset: 3),
        };

        foreach (var candle in candles)
        {
            await atr.SendAsync(candle);
        }

        atr.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("ATR"));
        Assert.False(results[1].Indicators.ContainsKey("ATR"));
        Assert.False(results[2].Indicators.ContainsKey("ATR"));
        Assert.Equal(5m, results[3].Indicators["ATR"]);
    }

    [Fact]
    public async Task Natr_WithValidPeriod_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var natr = VolatilityIndicators.Natr("NATR", 3);
        var sink = CreateSink(results);
        natr.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 12m, 8m, 10m, dayOffset: 0),
            MakeCandle(10m, 14m, 9m, 13m, dayOffset: 1),
            MakeCandle(13m, 16m, 11m, 15m, dayOffset: 2),
            MakeCandle(15m, 15m, 10m, 12m, dayOffset: 3),
        };

        foreach (var candle in candles)
        {
            await natr.SendAsync(candle);
        }

        natr.Complete();
        await sink.Completion;

        Assert.Equal(41.67m, Math.Round(results[3].Indicators["NATR"], 2));
    }

    [Fact]
    public async Task BBands_WithValidPeriod_ReturnsExpectedBands()
    {
        var results = new List<EnrichedCandle>();
        var bbands = VolatilityIndicators.BBands("BB", 3, 2m);
        var sink = CreateSink(results);
        bbands.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 10m, 10m, 10m, dayOffset: 0),
            MakeCandle(30m, 30m, 30m, 30m, dayOffset: 1),
            MakeCandle(10m, 10m, 10m, 10m, dayOffset: 2),
            MakeCandle(30m, 30m, 30m, 30m, dayOffset: 3),
        };

        foreach (var candle in candles)
        {
            await bbands.SendAsync(candle);
        }

        bbands.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("BB_Upper"));
        Assert.False(results[1].Indicators.ContainsKey("BB_Upper"));
        Assert.Equal(35.52m, Math.Round(results[2].Indicators["BB_Upper"], 2));
        Assert.Equal(16.67m, Math.Round(results[2].Indicators["BB_Middle"], 2));
        Assert.Equal(-2.19m, Math.Round(results[2].Indicators["BB_Lower"], 2));
    }

    [Fact]
    public void Atr_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VolatilityIndicators.Atr("ATR", 0));
    }

    [Fact]
    public void BBands_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VolatilityIndicators.BBands("BB", 0, 2m));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle MakeCandle(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = open,
        High = high,
        Low = low,
        Close = close,
        Volume = volume,
    });
}
