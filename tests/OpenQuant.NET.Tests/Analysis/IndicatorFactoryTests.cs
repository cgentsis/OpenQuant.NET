using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class IndicatorFactoryTests
{
    [Fact]
    public async Task Create_WithValidParameters_CreatesTransformBlock()
    {
        var block = IndicatorFactory.Create("TestIndicator", _ => 10m);

        Assert.IsType<TransformBlock<EnrichedCandle, EnrichedCandle>>(block);
    }

    [Fact]
    public async Task Create_WithCancellationRequested_DoesNotComputeValue()
    {
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync();
        var called = false;
        var candle = Enrich(10m);
        var block = IndicatorFactory.Create(
            "TestIndicator",
            _ =>
            {
                called = true;
                return 1m;
            },
            cts.Token);

        var accepted = await block.SendAsync(candle);
        block.Complete();

        Assert.True(accepted);
        Assert.False(called);
        Assert.False(candle.Indicators.ContainsKey("TestIndicator"));
    }

    [Fact]
    public async Task Create_WithValidComputationDelegate_ProducesExpectedResult()
    {
        var results = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(results.Add);
        var block = IndicatorFactory.Create("TestIndicator", _ => 15m);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(Enrich(10m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(15m, results[0].Indicators["TestIndicator"]);
    }

    [Fact]
    public async Task Create_WhenIndicatorIsWarmingUp_LeavesValueAbsent()
    {
        var results = new List<EnrichedCandle>();
        var sink = new ActionBlock<EnrichedCandle>(results.Add);
        var block = IndicatorFactory.Create("TestIndicator", _ => (decimal?)null);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(Enrich(10m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.False(results[0].Indicators.ContainsKey("TestIndicator"));
    }

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
