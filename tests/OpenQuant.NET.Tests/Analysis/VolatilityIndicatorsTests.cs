using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class VolatilityIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Atr_ProducesValueAfterPeriodPlusOneCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.Atr("ATR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetAtrCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(5, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("ATR"));
        Assert.False(results[1].Indicators.ContainsKey("ATR"));
        Assert.False(results[2].Indicators.ContainsKey("ATR"));
        Assert.True(results[3].Indicators.ContainsKey("ATR"));
    }

    [Fact]
    public async Task Atr_ConstantRangeCandles_EqualsRange()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.Atr("ATR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetConstantRangeCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(4m, results[3].Indicators["ATR"]);
    }

    [Fact]
    public async Task Natr_ProducesValueAndIsPositive()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.Natr("NATR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetAtrCandles());
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("NATR"));
        Assert.False(results[1].Indicators.ContainsKey("NATR"));
        Assert.False(results[2].Indicators.ContainsKey("NATR"));
        Assert.True(results[3].Indicators.ContainsKey("NATR"));
        Assert.True(results[3].Indicators["NATR"] > 0m);
    }

    [Fact]
    public async Task BBands_ProducesAllOutputKeys()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.BBands("BB", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetBollingerBandCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(5, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("BB_Upper"));
        Assert.False(results[1].Indicators.ContainsKey("BB_Middle"));
        Assert.False(results[1].Indicators.ContainsKey("BB_Lower"));
        Assert.True(results[2].Indicators.ContainsKey("BB_Upper"));
        Assert.True(results[2].Indicators.ContainsKey("BB_Middle"));
        Assert.True(results[2].Indicators.ContainsKey("BB_Lower"));
    }

    [Fact]
    public async Task BBands_BandsAreOrdered()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.BBands("BB", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetBollingerBandCandles());
        block.Complete();
        await sink.Completion;

        var upper = results[^1].Indicators["BB_Upper"];
        var middle = results[^1].Indicators["BB_Middle"];
        var lower = results[^1].Indicators["BB_Lower"];

        Assert.True(upper >= middle);
        Assert.True(middle >= lower);
    }

    [Fact]
    public async Task BBands_ConstantClose_HasNoDeviation()
    {
        var results = new List<EnrichedCandle>();
        var block = VolatilityIndicators.BBands("BB", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetConstantCloseCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(15m, results[2].Indicators["BB_Upper"]);
        Assert.Equal(15m, results[2].Indicators["BB_Middle"]);
        Assert.Equal(15m, results[2].Indicators["BB_Lower"]);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static async Task SendCandles(ITargetBlock<EnrichedCandle> block, IEnumerable<EnrichedCandle> candles)
    {
        foreach (var candle in candles)
        {
            await block.SendAsync(candle);
        }
    }

    private static EnrichedCandle[] GetAtrCandles() =>
    [
        MakeCandle(12m, 14m, 11m, 13m, 1000, 0),
        MakeCandle(13m, 15m, 12m, 14m, 1300, 1),
        MakeCandle(14m, 16m, 13m, 15m, 1600, 2),
        MakeCandle(15m, 18m, 14m, 17m, 1900, 3),
        MakeCandle(17m, 19m, 15m, 18m, 2200, 4),
    ];

    private static EnrichedCandle[] GetConstantRangeCandles() =>
    [
        MakeCandle(12m, 14m, 10m, 12m, 1000, 0),
        MakeCandle(13m, 15m, 11m, 13m, 1300, 1),
        MakeCandle(14m, 16m, 12m, 14m, 1600, 2),
        MakeCandle(15m, 17m, 13m, 15m, 1900, 3),
    ];

    private static EnrichedCandle[] GetBollingerBandCandles() =>
    [
        MakeCandle(12m, 13m, 11m, 12m, 1000, 0),
        MakeCandle(13m, 14m, 12m, 13m, 1500, 1),
        MakeCandle(15m, 16m, 14m, 15m, 2000, 2),
        MakeCandle(16m, 17m, 15m, 16m, 2500, 3),
        MakeCandle(18m, 19m, 17m, 18m, 3000, 4),
    ];

    private static EnrichedCandle[] GetConstantCloseCandles() =>
    [
        MakeCandle(15m, 16m, 14m, 15m, 1000, 0),
        MakeCandle(15m, 17m, 13m, 15m, 1500, 1),
        MakeCandle(15m, 18m, 12m, 15m, 2000, 2),
    ];

    private static EnrichedCandle MakeCandle(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) =>
        new(new Candle
        {
            Timestamp = BaseTime.AddDays(dayOffset),
            Open = open,
            High = high,
            Low = low,
            Close = close,
            Volume = volume,
        });
}
