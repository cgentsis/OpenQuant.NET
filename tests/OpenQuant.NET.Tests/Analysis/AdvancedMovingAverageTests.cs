using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class AdvancedMovingAverageTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task DEMA_ProducesValueAfterWarmupAndMatchesConstantValue()
    {
        const int period = 3;
        var results = new List<EnrichedCandle>();
        var block = MovingAverage.DEMA("DEMA", period);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            await block.SendAsync(MakeCandle(100m, 101m, 99m, 100m, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 4; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("DEMA"));
        }

        Assert.True(results[4].Indicators.ContainsKey("DEMA"));
        Assert.Equal(100m, results[4].Indicators["DEMA"]);
        Assert.Equal(100m, results[results.Count - 1].Indicators["DEMA"]);
    }

    [Fact]
    public async Task TEMA_ProducesValueAfterWarmupAndMatchesConstantValue()
    {
        const int period = 3;
        var results = new List<EnrichedCandle>();
        var block = MovingAverage.TEMA("TEMA", period);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            await block.SendAsync(MakeCandle(100m, 101m, 99m, 100m, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 6; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("TEMA"));
        }

        Assert.True(results[6].Indicators.ContainsKey("TEMA"));
        Assert.Equal(100m, results[6].Indicators["TEMA"]);
        Assert.Equal(100m, results[results.Count - 1].Indicators["TEMA"]);
    }

    [Fact]
    public async Task T3_ProducesValueAfterWarmup()
    {
        const int period = 3;
        var results = new List<EnrichedCandle>();
        var block = MovingAverage.T3("T3", period);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 15; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 12; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("T3"));
        }

        Assert.True(results[12].Indicators.ContainsKey("T3"));
        Assert.InRange(results[results.Count - 1].Indicators["T3"], 100m, 114m);
    }

    [Fact]
    public async Task TRIMA_ProducesValueAfterWarmupAndMatchesConstantValue()
    {
        const int period = 5;
        var results = new List<EnrichedCandle>();
        var block = MovingAverage.TRIMA("TRIMA", period);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 10; i++)
        {
            await block.SendAsync(MakeCandle(80m, 81m, 79m, 80m, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 4; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("TRIMA"));
        }

        Assert.True(results[4].Indicators.ContainsKey("TRIMA"));
        Assert.Equal(80m, results[4].Indicators["TRIMA"]);
        Assert.Equal(80m, results[results.Count - 1].Indicators["TRIMA"]);
    }

    [Fact]
    public async Task KAMA_ProducesValueAfterWarmupAndStaysWithinTrendRange()
    {
        const int period = 5;
        var results = new List<EnrichedCandle>();
        var block = MovingAverage.KAMA("KAMA", period);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        for (var i = 0; i < 12; i++)
        {
            var close = 100m + i;
            await block.SendAsync(MakeCandle(close - 1m, close + 1m, close - 2m, close, dayOffset: i));
        }

        block.Complete();
        await sink.Completion;

        for (var i = 0; i < 5; i++)
        {
            Assert.False(results[i].Indicators.ContainsKey("KAMA"));
        }

        Assert.True(results[5].Indicators.ContainsKey("KAMA"));
        Assert.True(results[results.Count - 1].Indicators.ContainsKey("KAMA"));
        Assert.True(results[results.Count - 1].Indicators["KAMA"] > results[5].Indicators["KAMA"]);
        Assert.InRange(results[results.Count - 1].Indicators["KAMA"], results[5].Indicators["KAMA"], 111m);
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
