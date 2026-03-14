using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class CandlestickPatternsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Doji_DetectsSmallBodyCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Doji("DOJI");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 15m, 5m, 10.3m));
        block.Complete();
        await sink.Completion;

        Assert.True(results[0].Indicators.ContainsKey("DOJI"));
        Assert.Equal(100m, results[0].Indicators["DOJI"]);
    }

    [Fact]
    public async Task Doji_DoesNotDetectLargeBodyCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Doji("DOJI");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 15m, 5m, 13m));
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("DOJI"));
    }

    [Fact]
    public async Task Hammer_DetectsHammerShape()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Hammer("HAMMER");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(12m, 12.2m, 5m, 11m));
        block.Complete();
        await sink.Completion;

        Assert.True(results[0].Indicators.ContainsKey("HAMMER"));
        Assert.Equal(100m, results[0].Indicators["HAMMER"]);
    }

    [Fact]
    public async Task Engulfing_DetectsBullishPattern()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Engulfing("ENGULFING");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(12m, 12.5m, 9.5m, 10m, dayOffset: 0));
        await block.SendAsync(MakeCandle(9m, 13m, 8.5m, 13m, dayOffset: 1));
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("ENGULFING"));
        Assert.True(results[1].Indicators.ContainsKey("ENGULFING"));
        Assert.Equal(100m, results[1].Indicators["ENGULFING"]);
    }

    [Fact]
    public async Task Engulfing_DoesNotDetectRandomCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Engulfing("ENGULFING");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 11m, 9m, 10.5m, dayOffset: 0));
        await block.SendAsync(MakeCandle(10.6m, 11.2m, 10m, 10.8m, dayOffset: 1));
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("ENGULFING"));
        Assert.False(results[1].Indicators.ContainsKey("ENGULFING"));
    }

    [Fact]
    public async Task MorningStar_DetectsBullishPattern()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.MorningStar("MORNINGSTAR");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(20m, 21m, 16m, 16.5m, dayOffset: 0));
        await block.SendAsync(MakeCandle(15m, 15.5m, 14.5m, 15m, dayOffset: 1));
        await block.SendAsync(MakeCandle(16m, 21m, 15.5m, 20m, dayOffset: 2));
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("MORNINGSTAR"));
        Assert.False(results[1].Indicators.ContainsKey("MORNINGSTAR"));
        Assert.True(results[2].Indicators.ContainsKey("MORNINGSTAR"));
        Assert.Equal(100m, results[2].Indicators["MORNINGSTAR"]);
    }

    [Fact]
    public async Task ThreeWhiteSoldiers_DetectsBullishSequence()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.ThreeWhiteSoldiers("3WS");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 12.5m, 9.5m, 12m, dayOffset: 0));
        await block.SendAsync(MakeCandle(11m, 13.5m, 10.5m, 13m, dayOffset: 1));
        await block.SendAsync(MakeCandle(12m, 14.5m, 11.5m, 14m, dayOffset: 2));
        block.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("3WS"));
        Assert.False(results[1].Indicators.ContainsKey("3WS"));
        Assert.True(results[2].Indicators.ContainsKey("3WS"));
        Assert.Equal(100m, results[2].Indicators["3WS"]);
    }

    [Fact]
    public async Task Marubozu_DetectsFullBodyCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = CandlestickPatterns.Marubozu("MARUBOZU");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 20m, 10m, 20m));
        block.Complete();
        await sink.Completion;

        Assert.True(results[0].Indicators.ContainsKey("MARUBOZU"));
        Assert.Equal(100m, results[0].Indicators["MARUBOZU"]);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

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
