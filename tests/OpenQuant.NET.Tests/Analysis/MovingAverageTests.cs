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

    [Fact]
    public async Task EMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // k = 2/(3+1) = 0.5
        // SMA seed = (10+20+30)/3 = 20
        // EMA(40) = 40*0.5 + 20*0.5 = 30
        // EMA(50) = 50*0.5 + 30*0.5 = 40
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.EMAActionBlockFactory(3, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(20m, 1));
        await block.SendAsync(MakeCandle(30m, 2));
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
    public async Task EMA_WithPeriodLargerThanData_ProducesNoOutput()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.EMAActionBlockFactory(5, output);

        await block.SendAsync(MakeCandle(10m));
        await block.SendAsync(MakeCandle(20m));
        block.Complete();
        await block.Completion;

        Assert.False(output.TryReceive(out _));
    }

    [Fact]
    public void EMA_WithPeriodLessThanOne_Throws()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.EMAActionBlockFactory(0, output));
    }

    [Fact]
    public async Task WMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // Period 3, weights 1,2,3, divisor = 6
        // (6*1+12*2+18*3)/6 = 84/6 = 14
        // (12*1+18*2+24*3)/6 = 120/6 = 20
        // (18*1+24*2+30*3)/6 = 156/6 = 26
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.WMAActionBlockFactory(3, output);

        await block.SendAsync(MakeCandle(6m, 0));
        await block.SendAsync(MakeCandle(12m, 1));
        await block.SendAsync(MakeCandle(18m, 2));
        await block.SendAsync(MakeCandle(24m, 3));
        await block.SendAsync(MakeCandle(30m, 4));
        block.Complete();
        await block.Completion;

        var results = Drain(output);

        Assert.Equal(3, results.Count);
        Assert.Equal(14m, results[0].Value);
        Assert.Equal(20m, results[1].Value);
        Assert.Equal(26m, results[2].Value);
    }

    [Fact]
    public async Task WMA_WithPeriodLargerThanData_ProducesNoOutput()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.WMAActionBlockFactory(5, output);

        await block.SendAsync(MakeCandle(10m));
        await block.SendAsync(MakeCandle(20m));
        block.Complete();
        await block.Completion;

        Assert.False(output.TryReceive(out _));
    }

    [Fact]
    public void WMA_WithPeriodLessThanOne_Throws()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.WMAActionBlockFactory(0, output));
    }

    [Fact]
    public async Task HMA_WithValidPeriod_ReturnsCorrectValues()
    {
        // Period 4: halfPeriod=2, sqrtPeriod=2
        // Linear data produces HMA that tracks perfectly.
        // First HMA output at index 4 (5th candle): 50
        // Then: 60, 70
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.HMAActionBlockFactory(4, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(20m, 1));
        await block.SendAsync(MakeCandle(30m, 2));
        await block.SendAsync(MakeCandle(40m, 3));
        await block.SendAsync(MakeCandle(50m, 4));
        await block.SendAsync(MakeCandle(60m, 5));
        await block.SendAsync(MakeCandle(70m, 6));
        block.Complete();
        await block.Completion;

        var results = Drain(output);

        Assert.Equal(3, results.Count);
        Assert.Equal(50m, Math.Round(results[0].Value, 20));
        Assert.Equal(60m, Math.Round(results[1].Value, 20));
        Assert.Equal(70m, Math.Round(results[2].Value, 20));
    }

    [Fact]
    public async Task HMA_WithInsufficientData_ProducesNoOutput()
    {
        // Period 4 requires 4 + sqrt(4) - 1 = 5 candles minimum
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();
        var block = MovingAverage.HMAActionBlockFactory(4, output);

        await block.SendAsync(MakeCandle(10m, 0));
        await block.SendAsync(MakeCandle(20m, 1));
        await block.SendAsync(MakeCandle(30m, 2));
        await block.SendAsync(MakeCandle(40m, 3));
        block.Complete();
        await block.Completion;

        Assert.False(output.TryReceive(out _));
    }

    [Fact]
    public void HMA_WithPeriodLessThanTwo_Throws()
    {
        var output = new BufferBlock<(DateTimeOffset Timestamp, decimal Value)>();

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.HMAActionBlockFactory(1, output));
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
