using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class MovingAverageTests
{
    private static Quote MakeQuote(decimal close, int dayOffset = 0) => new()
    {
        Symbol = "TEST",
        Timestamp = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero).AddDays(dayOffset),
        Open = close,
        High = close,
        Low = close,
        Close = close,
        Volume = 1000
    };

    [Fact]
    public void Simple_WithValidPeriod_ReturnsCorrectAverages()
    {
        var quotes = new[]
        {
            MakeQuote(10m, 0),
            MakeQuote(20m, 1),
            MakeQuote(30m, 2),
            MakeQuote(40m, 3),
            MakeQuote(50m, 4),
        };

        var result = MovingAverage.Simple(quotes, 3);

        Assert.Equal(3, result.Count);
        Assert.Equal(20m, result[0].Value);  // (10+20+30)/3
        Assert.Equal(30m, result[1].Value);  // (20+30+40)/3
        Assert.Equal(40m, result[2].Value);  // (30+40+50)/3
    }

    [Fact]
    public void Simple_WithPeriodLargerThanData_ReturnsEmpty()
    {
        var quotes = new[] { MakeQuote(10m), MakeQuote(20m) };

        var result = MovingAverage.Simple(quotes, 5);

        Assert.Empty(result);
    }

    [Fact]
    public void Simple_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            MovingAverage.Simple([], 0));
    }
}
