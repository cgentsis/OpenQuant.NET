using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class DirectionalMovementTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task PlusDM_ProducesValueAfterPeriodCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.PlusDM("PLUS_DM", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("PLUS_DM"));
        Assert.False(results[1].Indicators.ContainsKey("PLUS_DM"));
        Assert.False(results[2].Indicators.ContainsKey("PLUS_DM"));
        Assert.True(results[3].Indicators.ContainsKey("PLUS_DM"));
    }

    [Fact]
    public async Task MinusDM_ProducesValueAfterPeriodCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.MinusDM("MINUS_DM", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("MINUS_DM"));
        Assert.False(results[1].Indicators.ContainsKey("MINUS_DM"));
        Assert.False(results[2].Indicators.ContainsKey("MINUS_DM"));
        Assert.True(results[3].Indicators.ContainsKey("MINUS_DM"));
    }

    [Fact]
    public async Task PlusDI_ProducesValueAfterPeriodCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.PlusDI("PLUS_DI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("PLUS_DI"));
        Assert.False(results[1].Indicators.ContainsKey("PLUS_DI"));
        Assert.False(results[2].Indicators.ContainsKey("PLUS_DI"));
        Assert.True(results[3].Indicators.ContainsKey("PLUS_DI"));
    }

    [Fact]
    public async Task MinusDI_ProducesValueAfterPeriodCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.MinusDI("MINUS_DI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("MINUS_DI"));
        Assert.False(results[1].Indicators.ContainsKey("MINUS_DI"));
        Assert.False(results[2].Indicators.ContainsKey("MINUS_DI"));
        Assert.True(results[3].Indicators.ContainsKey("MINUS_DI"));
    }

    [Fact]
    public async Task Dx_ProducesValueAfterSufficientCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.Dx("DX", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("DX"));
        Assert.False(results[1].Indicators.ContainsKey("DX"));
        Assert.False(results[2].Indicators.ContainsKey("DX"));
        Assert.True(results[5].Indicators.ContainsKey("DX"));
    }

    [Fact]
    public async Task Adx_ProducesValueAfterTwoPeriods()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.Adx("ADX", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[4].Indicators.ContainsKey("ADX"));
        Assert.True(results[5].Indicators.ContainsKey("ADX"));
    }

    [Fact]
    public async Task Adxr_ProducesValueAfterThreePeriods()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.Adxr("ADXR", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[7].Indicators.ContainsKey("ADXR"));
        Assert.True(results[8].Indicators.ContainsKey("ADXR"));
    }

    [Fact]
    public async Task DirectionalIndicators_AreNonNegative()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.PlusDI("PLUS_DI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        var plusDi = results[^1].Indicators["PLUS_DI"];
        Assert.True(plusDi >= 0m);

        results = new List<EnrichedCandle>();
        block = DirectionalMovement.MinusDI("MINUS_DI", 3);
        sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        var minusDi = results[^1].Indicators["MINUS_DI"];
        Assert.True(minusDi >= 0m);
    }

    [Fact]
    public async Task Dx_IsBetweenZeroAndHundred()
    {
        var results = new List<EnrichedCandle>();
        var block = DirectionalMovement.Dx("DX", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetDirectionalMovementCandles());
        block.Complete();
        await sink.Completion;

        var value = results[^1].Indicators["DX"];
        Assert.InRange(value, 0m, 100m);
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

    private static EnrichedCandle[] GetDirectionalMovementCandles() =>
    [
        MakeCandle(12m, 14m, 11m, 13m, 1000, 0),
        MakeCandle(13m, 16m, 12m, 15m, 1200, 1),
        MakeCandle(15m, 15.5m, 11m, 12m, 1500, 2),
        MakeCandle(12.5m, 17m, 12m, 16m, 1800, 3),
        MakeCandle(16m, 17.5m, 12.5m, 13m, 2100, 4),
        MakeCandle(13m, 15m, 10.5m, 11m, 2400, 5),
        MakeCandle(11m, 13.5m, 10m, 12.5m, 2700, 6),
        MakeCandle(12.5m, 16m, 12m, 15m, 3000, 7),
        MakeCandle(15m, 18m, 14m, 17m, 3300, 8),
        MakeCandle(17m, 19m, 15.5m, 18m, 3600, 9),
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
