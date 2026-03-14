using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MovingMedianTests
{
    [Fact]
    public async Task Median_OddPeriod_ReturnsCorrectValues()
    {
        // Period 3: sorted windows > middle element
        // [10,30,20] sorted [10,20,30] > 20
        // [30,20,40] sorted [20,30,40] > 30
        // [20,40,50] sorted [20,40,50] > 40
        var results = new List<EnrichedCandle>();
        var median = MovingMedian.Median("Med", 3);
        var sink = CreateSink(results);
        median.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await median.SendAsync(Enrich(10m, 0));
        await median.SendAsync(Enrich(30m, 1));
        await median.SendAsync(Enrich(20m, 2));
        await median.SendAsync(Enrich(40m, 3));
        await median.SendAsync(Enrich(50m, 4));
        median.Complete();
        await sink.Completion;

        Assert.Equal(20m, results[2].Indicators["Med"]);
        Assert.Equal(30m, results[3].Indicators["Med"]);
        Assert.Equal(40m, results[4].Indicators["Med"]);
    }

    [Fact]
    public async Task Median_EvenPeriod_ReturnsAverageOfMiddleTwo()
    {
        // Period 4: average of two middle elements
        // [10,30,20,40] sorted [10,20,30,40] > (20+30)/2 = 25
        // [30,20,40,50] sorted [20,30,40,50] > (30+40)/2 = 35
        var results = new List<EnrichedCandle>();
        var median = MovingMedian.Median("Med", 4);
        var sink = CreateSink(results);
        median.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await median.SendAsync(Enrich(10m, 0));
        await median.SendAsync(Enrich(30m, 1));
        await median.SendAsync(Enrich(20m, 2));
        await median.SendAsync(Enrich(40m, 3));
        await median.SendAsync(Enrich(50m, 4));
        median.Complete();
        await sink.Completion;

        Assert.Equal(25m, results[3].Indicators["Med"]);
        Assert.Equal(35m, results[4].Indicators["Med"]);
    }

    [Fact]
    public async Task Median_WithPeriodLargerThanData_ProducesNoValues()
    {
        var results = new List<EnrichedCandle>();
        var median = MovingMedian.Median("Med", 5);
        var sink = CreateSink(results);
        median.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await median.SendAsync(Enrich(10m));
        await median.SendAsync(Enrich(20m));
        median.Complete();
        await sink.Completion;

        Assert.All(results, r => Assert.Empty(r.Indicators));
    }

    [Fact]
    public void Median_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingMedian.Median("Med", 0));
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
