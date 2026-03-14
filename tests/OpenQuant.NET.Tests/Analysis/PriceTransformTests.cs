using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class PriceTransformTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task AvgPrice_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.AvgPrice("AVGPRICE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 20m, 10m, 12m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(13m, results[0].Indicators["AVGPRICE"]); // (10+20+10+12)/4
    }

    [Fact]
    public async Task MedPrice_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.MedPrice("MEDPRICE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 20m, 10m, 12m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(15m, results[0].Indicators["MEDPRICE"]); // (20+10)/2
    }

    [Fact]
    public async Task TypPrice_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.TypPrice("TYPPRICE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        // Use values where (H+L+C) is divisible by 3
        await block.SendAsync(MakeCandle(10m, 20m, 7m, 6m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(11m, results[0].Indicators["TYPPRICE"]); // (20+7+6)/3
    }

    [Fact]
    public async Task WclPrice_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.WclPrice("WCLPRICE");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 20m, 6m, 12m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(12.5m, results[0].Indicators["WCLPRICE"]); // (20+6+12+12)/4
    }

    [Fact]
    public async Task Bop_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.Bop("BOP");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 18m, 8m, 12m));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.Equal(0.2m, results[0].Indicators["BOP"]); // (12-10)/(18-8)
    }

    [Fact]
    public async Task Bop_ZeroRange_ReturnsZero()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.Bop("BOP");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 10m, 10m, 10m));
        block.Complete();
        await sink.Completion;

        Assert.Equal(0m, results[0].Indicators["BOP"]);
    }

    [Fact]
    public async Task TrueRange_ComputesCorrectly()
    {
        var results = new List<EnrichedCandle>();
        var block = PriceTransform.TrueRange("TR");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await block.SendAsync(MakeCandle(10m, 20m, 10m, 12m, dayOffset: 0));
        await block.SendAsync(MakeCandle(12m, 22m, 11m, 18m, dayOffset: 1));
        await block.SendAsync(MakeCandle(18m, 24m, 14m, 16m, dayOffset: 2));
        block.Complete();
        await sink.Completion;

        Assert.Equal(3, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("TR"));
        Assert.Equal(11m, results[1].Indicators["TR"]);
        Assert.Equal(10m, results[2].Indicators["TR"]);
    }

    [Fact]
    public async Task AllPriceTransforms_ProduceValueOnFirstCandle()
    {
        // Price transforms (except TrueRange) are stateless
        var avgResults = new List<EnrichedCandle>();
        var avg = PriceTransform.AvgPrice("AVG");
        var avgSink = CreateSink(avgResults);
        avg.LinkTo(avgSink, new DataflowLinkOptions { PropagateCompletion = true });

        await avg.SendAsync(MakeCandle(10m, 20m, 5m, 15m));
        avg.Complete();
        await avgSink.Completion;

        Assert.True(avgResults[0].Indicators.ContainsKey("AVG"));
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
