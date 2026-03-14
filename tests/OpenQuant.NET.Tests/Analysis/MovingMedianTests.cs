using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MovingMedianTests
{
    [Fact]
    public async Task Median_OddPeriod_ReturnsCorrectValues()
    {
        // Period 3: sorted windows → middle element
        // [10,30,20] sorted [10,20,30] → 20
        // [30,20,40] sorted [20,30,40] → 30
        // [20,40,50] sorted [20,40,50] → 40
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingMedian.MedianActionBlockFactory(3, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(30m, 1));
        await block.SendAsync(MakeCandle(20m, 2));
        await block.SendAsync(MakeCandle(40m, 3));
        await block.SendAsync(MakeCandle(50m, 4));
        block.Complete();
        await block.Completion;

        var results = Drain(output);

        Assert.Equal(3, results.Count);
        Assert.Equal(20m, results[0].Value);
        Assert.Equal(30m, results[1].Value);
        Assert.Equal(40m, results[2].Value);
    }

    [Fact]
    public async Task Median_EvenPeriod_ReturnsAverageOfMiddleTwo()
    {
        // Period 4: average of two middle elements
        // [10,30,20,40] sorted [10,20,30,40] → (20+30)/2 = 25
        // [30,20,40,50] sorted [20,30,40,50] → (30+40)/2 = 35
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingMedian.MedianActionBlockFactory(4, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(30m, 1));
        await block.SendAsync(MakeCandle(20m, 2));
        await block.SendAsync(MakeCandle(40m, 3));
        await block.SendAsync(MakeCandle(50m, 4));
        block.Complete();
        await block.Completion;

        var results = Drain(output);

        Assert.Equal(2, results.Count);
        Assert.Equal(25m, results[0].Value);
        Assert.Equal(35m, results[1].Value);
    }

    [Fact]
    public async Task Median_WithPeriodLargerThanData_ProducesNoOutput()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingMedian.MedianActionBlockFactory(5, output);

        await block.SendAsync(MakeCandle(10m));
        await block.SendAsync(MakeCandle(20m));
        block.Complete();
        await block.Completion;

        Assert.False(output.TryReceive(out _));
    }

    [Fact]
    public void Median_WithPeriodLessThanOne_Throws()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingMedian.MedianActionBlockFactory(0, output));
    }

    private static List<(DateTimeOffset Timestamp, decimal Value)> Drain(
        BufferBlock<(DateTimeOffset Timestamp, decimal Value)> buffer)
    {
        var results = new List<(DateTimeOffset Timestamp, decimal Value)>();
        while (buffer.TryReceive(out var item))
        {
            results.Add(item);
        }

        return results;
    }

    private static Candle MakeCandle(decimal close, int dayOffset = 0) => new()
    {
        Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000,
    };
}
