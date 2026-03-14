using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class AdvancedMovingAverageTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DEMA_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var dema = MovingAverage.DEMA("DEMA", 3);
        var sink = CreateSink(results);
        dema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
            Enrich(70m, 6),
            Enrich(80m, 7),
        };

        foreach (var candle in candles)
        {
            await dema.SendAsync(candle);
        }

        dema.Complete();
        await sink.Completion;

        Assert.Equal(8, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("DEMA"));
        Assert.False(results[1].Indicators.ContainsKey("DEMA"));
        Assert.False(results[2].Indicators.ContainsKey("DEMA"));
        Assert.False(results[3].Indicators.ContainsKey("DEMA"));
        Assert.Equal(50m, Math.Round(results[4].Indicators["DEMA"], 2));
        Assert.Equal(70m, Math.Round(results[6].Indicators["DEMA"], 2));
        Assert.Equal(80m, Math.Round(results[7].Indicators["DEMA"], 2));
    }

    [Fact]
    public async Task TEMA_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var tema = MovingAverage.TEMA("TEMA", 2);
        var sink = CreateSink(results);
        tema.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
        };

        foreach (var candle in candles)
        {
            await tema.SendAsync(candle);
        }

        tema.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("TEMA"));
        Assert.False(results[1].Indicators.ContainsKey("TEMA"));
        Assert.False(results[2].Indicators.ContainsKey("TEMA"));
        Assert.Equal(40m, Math.Round(results[3].Indicators["TEMA"], 2));
        Assert.Equal(50m, Math.Round(results[4].Indicators["TEMA"], 2));
        Assert.Equal(60m, Math.Round(results[5].Indicators["TEMA"], 2));
    }

    [Fact]
    public async Task TRIMA_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var trima = MovingAverage.TRIMA("TRIMA", 4);
        var sink = CreateSink(results);
        trima.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
        };

        foreach (var candle in candles)
        {
            await trima.SendAsync(candle);
        }

        trima.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("TRIMA"));
        Assert.False(results[1].Indicators.ContainsKey("TRIMA"));
        Assert.False(results[2].Indicators.ContainsKey("TRIMA"));
        Assert.Equal(25m, results[3].Indicators["TRIMA"]);
        Assert.Equal(35m, results[4].Indicators["TRIMA"]);
        Assert.Equal(45m, results[5].Indicators["TRIMA"]);
    }

    [Fact]
    public async Task KAMA_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var kama = MovingAverage.KAMA("KAMA", 3);
        var sink = CreateSink(results);
        kama.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
            Enrich(70m, 6),
        };

        foreach (var candle in candles)
        {
            await kama.SendAsync(candle);
        }

        kama.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("KAMA"));
        Assert.False(results[1].Indicators.ContainsKey("KAMA"));
        Assert.False(results[2].Indicators.ContainsKey("KAMA"));
        Assert.Equal(40m, results[3].Indicators["KAMA"]);
        Assert.Equal(44.4444m, Math.Round(results[4].Indicators["KAMA"], 4));
        Assert.Equal(59.6433m, Math.Round(results[6].Indicators["KAMA"], 4));
    }

    [Fact]
    public async Task T3_WithValidPeriod_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var t3 = MovingAverage.T3("T3", 2, 0.7m);
        var sink = CreateSink(results);
        t3.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            Enrich(10m, 0),
            Enrich(20m, 1),
            Enrich(30m, 2),
            Enrich(40m, 3),
            Enrich(50m, 4),
            Enrich(60m, 5),
            Enrich(70m, 6),
            Enrich(80m, 7),
            Enrich(90m, 8),
            Enrich(100m, 9),
            Enrich(110m, 10),
            Enrich(120m, 11),
        };

        foreach (var candle in candles)
        {
            await t3.SendAsync(candle);
        }

        t3.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("T3"));
        Assert.False(results[1].Indicators.ContainsKey("T3"));
        Assert.False(results[2].Indicators.ContainsKey("T3"));
        Assert.False(results[3].Indicators.ContainsKey("T3"));
        Assert.False(results[4].Indicators.ContainsKey("T3"));
        Assert.False(results[5].Indicators.ContainsKey("T3"));
        Assert.True(results[6].Indicators.ContainsKey("T3"));
        Assert.Equal(65.5m, Math.Round(results[6].Indicators["T3"], 1));
        Assert.Equal(105.5m, Math.Round(results[10].Indicators["T3"], 1));
        Assert.Equal(115.5m, Math.Round(results[11].Indicators["T3"], 1));
    }

    [Fact]
    public void DEMA_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.DEMA("DEMA", 0));
    }

    [Fact]
    public void TRIMA_WithPeriodLessThanTwo_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.TRIMA("TRIMA", 1));
    }

    [Fact]
    public void KAMA_WithPeriodLessThanTwo_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.KAMA("KAMA", 1));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle Enrich(decimal close, int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    });
}
