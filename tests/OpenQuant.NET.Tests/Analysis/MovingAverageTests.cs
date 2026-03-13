using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MovingAverageTests
{
    [Fact]
    public async Task Simple_WithValidPeriod_ReturnsCorrectAverages()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.SMAActionBlockFactory(3, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(20m, 1));
        await block.SendAsync(MakeCandle(30m, 2));
        await block.SendAsync(MakeCandle(40m, 3));
        await block.SendAsync(MakeCandle(50m, 4));
        block.Complete();
        await block.Completion;

        var results = new List<(DateTimeOffset Timestamp, decimal Value)>();
        while (output.TryReceive(out var item))
        {
            results.Add(item);
        }

        Assert.Equal(3, results.Count);
        Assert.Equal(20m, results[0].Value);  // (10+20+30)/3
        Assert.Equal(30m, results[1].Value);  // (20+30+40)/3
        Assert.Equal(40m, results[2].Value);  // (30+40+50)/3
    }

    [Fact]
    public async Task Simple_WithPeriodLargerThanData_ProducesNoOutput()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.SMAActionBlockFactory(5, output);

        await block.SendAsync(MakeCandle(10m));
        await block.SendAsync(MakeCandle(20m));
        block.Complete();
        await block.Completion;

        Assert.False(output.TryReceive(out _));
    }

    [Fact]
    public void Simple_WithPeriodLessThanOne_Throws()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.SMAActionBlockFactory(0, output));
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
