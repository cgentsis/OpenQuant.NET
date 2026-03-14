using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MovingAverageTests
{
    [Fact]
    public async Task SMA_WithValidPeriod_ReturnsCorrectAverages()
    {
        var results = new List<EnrichedCandle>();
        var sma = MovingAverage.SMA("SMA", 3);
        var sink = CreateSink(results);
        sma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await sma.SendAsync(Enrich(10m, 0));
        await sma.SendAsync(Enrich(20m, 1));
        await sma.SendAsync(Enrich(30m, 2));
        await sma.SendAsync(Enrich(40m, 3));
        await sma.SendAsync(Enrich(50m, 4));
        sma.Complete();
        await sink.Completion;

        Assert.Equal(5, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("SMA"));
        Assert.False(results[1].Indicators.ContainsKey("SMA"));
        Assert.Equal(20m, results[2].Indicators["SMA"]);   // (10+20+30)/3
        Assert.Equal(30m, results[3].Indicators["SMA"]);   // (20+30+40)/3
        Assert.Equal(40m, results[4].Indicators["SMA"]);   // (30+40+50)/3
    }

    [Fact]
    public async Task SMA_WithPeriodLargerThanData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var sma = MovingAverage.SMA("SMA", 5);
        var sink = CreateSink(results);
        sma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await sma.SendAsync(Enrich(10m));
        await sma.SendAsync(Enrich(20m));
        sma.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Fact]
    public void SMA_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.SMA("SMA", 0));
    }

    [Fact]
    public async Task EMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // k = 2/(3+1) = 0.5
        // SMA seed = (10+20+30)/3 = 20
        // EMA(40) = 40*0.5 + 20*0.5 = 30
        // EMA(50) = 50*0.5 + 30*0.5 = 40
        var results = new List<EnrichedCandle>();
        var ema = MovingAverage.EMA("EMA", 3);
        var sink = CreateSink(results);
        ema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await ema.SendAsync(Enrich(10m, 0));
        await ema.SendAsync(Enrich(20m, 1));
        await ema.SendAsync(Enrich(30m, 2));
        await ema.SendAsync(Enrich(40m, 3));
        await ema.SendAsync(Enrich(50m, 4));
        ema.Complete();
        await sink.Completion;

        Assert.Equal(20m, results[2].Indicators["EMA"]);
        Assert.Equal(30m, results[3].Indicators["EMA"]);
        Assert.Equal(40m, results[4].Indicators["EMA"]);
    }

    [Fact]
    public async Task EMA_WithPeriodLargerThanData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var ema = MovingAverage.EMA("EMA", 5);
        var sink = CreateSink(results);
        ema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await ema.SendAsync(Enrich(10m));
        await ema.SendAsync(Enrich(20m));
        ema.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Fact]
    public void EMA_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.EMA("EMA", 0));
    }

    [Fact]
    public async Task WMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // Period 3, weights 1,2,3, divisor = 6
        // (6*1+12*2+18*3)/6 = 84/6 = 14
        // (12*1+18*2+24*3)/6 = 120/6 = 20
        // (18*1+24*2+30*3)/6 = 156/6 = 26
        var results = new List<EnrichedCandle>();
        var wma = MovingAverage.WMA("WMA", 3);
        var sink = CreateSink(results);
        wma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await wma.SendAsync(Enrich(6m, 0));
        await wma.SendAsync(Enrich(12m, 1));
        await wma.SendAsync(Enrich(18m, 2));
        await wma.SendAsync(Enrich(24m, 3));
        await wma.SendAsync(Enrich(30m, 4));
        wma.Complete();
        await sink.Completion;

        Assert.Equal(14m, results[2].Indicators["WMA"]);
        Assert.Equal(20m, results[3].Indicators["WMA"]);
        Assert.Equal(26m, results[4].Indicators["WMA"]);
    }

    [Fact]
    public async Task WMA_WithPeriodLargerThanData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var wma = MovingAverage.WMA("WMA", 5);
        var sink = CreateSink(results);
        wma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await wma.SendAsync(Enrich(10m));
        await wma.SendAsync(Enrich(20m));
        wma.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Fact]
    public void WMA_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.WMA("WMA", 0));
    }

    [Fact]
    public async Task HMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // Period 4: halfPeriod=2, sqrtPeriod=2
        // Linear data produces HMA that tracks perfectly.
        var results = new List<EnrichedCandle>();
        var hma = MovingAverage.HMA("HMA", 4);
        var sink = CreateSink(results);
        hma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await hma.SendAsync(Enrich(10m, 0));
        await hma.SendAsync(Enrich(20m, 1));
        await hma.SendAsync(Enrich(30m, 2));
        await hma.SendAsync(Enrich(40m, 3));
        await hma.SendAsync(Enrich(50m, 4));
        await hma.SendAsync(Enrich(60m, 5));
        await hma.SendAsync(Enrich(70m, 6));
        hma.Complete();
        await sink.Completion;

        var hmaResults = results.Where(r => r.Indicators.ContainsKey("HMA")).ToList();

        Assert.Equal(3, hmaResults.Count);
        Assert.Equal(50m, Math.Round(hmaResults[0].Indicators["HMA"], 20));
        Assert.Equal(60m, Math.Round(hmaResults[1].Indicators["HMA"], 20));
        Assert.Equal(70m, Math.Round(hmaResults[2].Indicators["HMA"], 20));
    }

    [Fact]
    public async Task HMA_WithInsufficientData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var hma = MovingAverage.HMA("HMA", 4);
        var sink = CreateSink(results);
        hma.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await hma.SendAsync(Enrich(10m, 0));
        await hma.SendAsync(Enrich(20m, 1));
        await hma.SendAsync(Enrich(30m, 2));
        await hma.SendAsync(Enrich(40m, 3));
        hma.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Fact]
    public void HMA_WithPeriodLessThanTwo_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.HMA("HMA", 1));
    }

    [Fact]
    public async Task Pipeline_ChainedBlocks_EnrichesCandles()
    {
        // Chain SMA(3) → EMA(3) via LinkTo
        var results = new List<EnrichedCandle>();
        var sma = MovingAverage.SMA("SMA3", 3);
        var ema = MovingAverage.EMA("EMA3", 3);
        var sink = CreateSink(results);

        sma.LinkTo(ema, new DataflowLinkOptions { PropagateCompletion = true });
        ema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await sma.SendAsync(Enrich(10m, 0));
        await sma.SendAsync(Enrich(20m, 1));
        await sma.SendAsync(Enrich(30m, 2));
        await sma.SendAsync(Enrich(25m, 3));
        await sma.SendAsync(Enrich(35m, 4));
        sma.Complete();
        await sink.Completion;

        Assert.Equal(5, results.Count);

        // SMA3 starts at index 2, EMA3 starts at index 2
        Assert.Equal(20m, results[2].Indicators["SMA3"]);
        Assert.Equal(20m, results[2].Indicators["EMA3"]);

        // k = 0.5; EMA(25) = 25*0.5 + 20*0.5 = 22.5
        Assert.Equal(25m, results[3].Indicators["SMA3"]);
        Assert.Equal(22.5m, results[3].Indicators["EMA3"]);

        // EMA(35) = 35*0.5 + 22.5*0.5 = 28.75
        Assert.Equal(30m, results[4].Indicators["SMA3"]);
        Assert.Equal(28.75m, results[4].Indicators["EMA3"]);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle Enrich(decimal close, int dayOffset = 0) => new(new Candle
    {
        Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    });
}
