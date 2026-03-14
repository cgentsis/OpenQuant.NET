using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class DirectionalMovementTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task PlusDM_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var plusDm = DirectionalMovement.PlusDM("PlusDM", 3);
        var sink = CreateSink(results);
        plusDm.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await plusDm.SendAsync(candle);
        }

        plusDm.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("PlusDM"));
        Assert.False(results[1].Indicators.ContainsKey("PlusDM"));
        Assert.False(results[2].Indicators.ContainsKey("PlusDM"));
        Assert.Equal(6m, results[3].Indicators["PlusDM"]);
        Assert.Equal(2.67m, Math.Round(results[6].Indicators["PlusDM"], 2));
    }

    [Fact]
    public async Task MinusDM_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var minusDm = DirectionalMovement.MinusDM("MinusDM", 3);
        var sink = CreateSink(results);
        minusDm.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await minusDm.SendAsync(candle);
        }

        minusDm.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("MinusDM"));
        Assert.False(results[1].Indicators.ContainsKey("MinusDM"));
        Assert.False(results[2].Indicators.ContainsKey("MinusDM"));
        Assert.Equal(0m, results[3].Indicators["MinusDM"]);
        Assert.Equal(4m, results[7].Indicators["MinusDM"]);
    }

    [Fact]
    public async Task PlusDI_WithValidPeriod_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var plusDi = DirectionalMovement.PlusDI("PlusDI", 3);
        var sink = CreateSink(results);
        plusDi.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await plusDi.SendAsync(candle);
        }

        plusDi.Complete();
        await sink.Completion;

        Assert.Equal(50m, Math.Round(results[3].Indicators["PlusDI"], 2));

        for (var i = 3; i < results.Count; i++)
        {
            Assert.True(results[i].Indicators.ContainsKey("PlusDI"));
            Assert.InRange(results[i].Indicators["PlusDI"], 0m, 100m);
        }
    }

    [Fact]
    public async Task MinusDI_WithValidPeriod_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var minusDi = DirectionalMovement.MinusDI("MinusDI", 3);
        var sink = CreateSink(results);
        minusDi.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await minusDi.SendAsync(candle);
        }

        minusDi.Complete();
        await sink.Completion;

        Assert.Equal(20.93m, Math.Round(results[6].Indicators["MinusDI"], 2));

        for (var i = 3; i < results.Count; i++)
        {
            Assert.True(results[i].Indicators.ContainsKey("MinusDI"));
            Assert.InRange(results[i].Indicators["MinusDI"], 0m, 100m);
        }
    }

    [Fact]
    public async Task Dx_WithValidPeriod_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var dx = DirectionalMovement.Dx("DX", 3);
        var sink = CreateSink(results);
        dx.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await dx.SendAsync(candle);
        }

        dx.Complete();
        await sink.Completion;

        Assert.Equal(100m, results[3].Indicators["DX"]);
        Assert.Equal(38.46m, Math.Round(results[7].Indicators["DX"], 2));

        for (var i = 3; i < results.Count; i++)
        {
            Assert.True(results[i].Indicators.ContainsKey("DX"));
            Assert.InRange(results[i].Indicators["DX"], 0m, 100m);
        }
    }

    [Fact]
    public async Task Adx_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var adx = DirectionalMovement.Adx("ADX", 3);
        var sink = CreateSink(results);
        adx.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await adx.SendAsync(candle);
        }

        adx.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("ADX"));
        Assert.False(results[1].Indicators.ContainsKey("ADX"));
        Assert.False(results[2].Indicators.ContainsKey("ADX"));
        Assert.False(results[3].Indicators.ContainsKey("ADX"));
        Assert.False(results[4].Indicators.ContainsKey("ADX"));
        Assert.Equal(100m, Math.Round(results[5].Indicators["ADX"], 2));
        Assert.Equal(58.29m, Math.Round(results[9].Indicators["ADX"], 2));
    }

    [Fact]
    public async Task Adxr_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var adxr = DirectionalMovement.Adxr("ADXR", 3);
        var sink = CreateSink(results);
        adxr.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        foreach (var candle in CreateDirectionalCandles())
        {
            await adxr.SendAsync(candle);
        }

        adxr.Complete();
        await sink.Completion;

        for (var i = 0; i < 8; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("ADXR"));
        }

        Assert.Equal(78.05m, Math.Round(results[8].Indicators["ADXR"], 2));
        Assert.Equal(63.46m, Math.Round(results[9].Indicators["ADXR"], 2));
    }

    [Fact]
    public void PlusDM_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            DirectionalMovement.PlusDM("PlusDM", 0));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle[] CreateDirectionalCandles() =>
    [
        MakeCandle(10m, 12m, 9m, 11m, dayOffset: 0),
        MakeCandle(11m, 14m, 10m, 13m, dayOffset: 1),
        MakeCandle(13m, 16m, 12m, 15m, dayOffset: 2),
        MakeCandle(15m, 18m, 14m, 17m, dayOffset: 3),
        MakeCandle(17m, 20m, 16m, 19m, dayOffset: 4),
        MakeCandle(19m, 21m, 15m, 16m, dayOffset: 5),
        MakeCandle(16m, 17m, 12m, 13m, dayOffset: 6),
        MakeCandle(13m, 14m, 10m, 11m, dayOffset: 7),
        MakeCandle(11m, 13m, 9m, 10m, dayOffset: 8),
        MakeCandle(10m, 12m, 8m, 9m, dayOffset: 9),
    ];

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
