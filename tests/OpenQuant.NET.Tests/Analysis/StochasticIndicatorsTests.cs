using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class StochasticIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Stoch_WithValidPeriods_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var stoch = StochasticIndicators.Stoch("Stoch", 3, 1, 1);
        var sink = CreateSink(results);
        stoch.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = CreateStochasticCandles();

        foreach (var candle in candles)
        {
            await stoch.SendAsync(candle);
        }

        stoch.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("Stoch_K"));
        Assert.False(results[1].Indicators.ContainsKey("Stoch_K"));
        Assert.Equal(87.5m, Math.Round(results[2].Indicators["Stoch_K"], 2));
        Assert.Equal(87.5m, Math.Round(results[2].Indicators["Stoch_D"], 2));
        Assert.Equal(42.86m, Math.Round(results[3].Indicators["Stoch_K"], 2));
        Assert.Equal(42.86m, Math.Round(results[3].Indicators["Stoch_D"], 2));
    }

    [Fact]
    public async Task StochF_WithValidPeriods_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var stochF = StochasticIndicators.StochF("StochF", 3, 1);
        var sink = CreateSink(results);
        stochF.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = CreateStochasticCandles();

        foreach (var candle in candles)
        {
            await stochF.SendAsync(candle);
        }

        stochF.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("StochF_K"));
        Assert.False(results[1].Indicators.ContainsKey("StochF_K"));
        Assert.Equal(87.5m, Math.Round(results[2].Indicators["StochF_K"], 2));
        Assert.Equal(87.5m, Math.Round(results[2].Indicators["StochF_D"], 2));
        Assert.Equal(42.86m, Math.Round(results[3].Indicators["StochF_K"], 2));
        Assert.Equal(42.86m, Math.Round(results[3].Indicators["StochF_D"], 2));
    }

    [Fact]
    public async Task StochRsi_WithValidPeriods_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var stochRsi = StochasticIndicators.StochRsi("StochRsi", 3, 3, 1, 1);
        var sink = CreateSink(results);
        stochRsi.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(44m, 44m, 44m, 44m, dayOffset: 0),
            MakeCandle(44.34m, 44.34m, 44.34m, 44.34m, dayOffset: 1),
            MakeCandle(44.09m, 44.09m, 44.09m, 44.09m, dayOffset: 2),
            MakeCandle(43.61m, 43.61m, 43.61m, 43.61m, dayOffset: 3),
            MakeCandle(44.33m, 44.33m, 44.33m, 44.33m, dayOffset: 4),
            MakeCandle(44.83m, 44.83m, 44.83m, 44.83m, dayOffset: 5),
            MakeCandle(45.10m, 45.10m, 45.10m, 45.10m, dayOffset: 6),
            MakeCandle(44.02m, 44.02m, 44.02m, 44.02m, dayOffset: 7),
            MakeCandle(44.17m, 44.17m, 44.17m, 44.17m, dayOffset: 8),
            MakeCandle(43.56m, 43.56m, 43.56m, 43.56m, dayOffset: 9),
        };

        foreach (var candle in candles)
        {
            await stochRsi.SendAsync(candle);
        }

        stochRsi.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("StochRsi_K"));
        Assert.False(results[1].Indicators.ContainsKey("StochRsi_K"));
        Assert.False(results[2].Indicators.ContainsKey("StochRsi_K"));
        Assert.False(results[3].Indicators.ContainsKey("StochRsi_K"));
        Assert.False(results[4].Indicators.ContainsKey("StochRsi_K"));
        Assert.Equal(100m, Math.Round(results[5].Indicators["StochRsi_K"], 2));
        Assert.Equal(100m, Math.Round(results[5].Indicators["StochRsi_D"], 2));
        Assert.Equal(14.49m, Math.Round(results[8].Indicators["StochRsi_K"], 2));

        for (var i = 5; i < results.Count; i++)
        {
            Assert.True(results[i].Indicators.ContainsKey("StochRsi_K"));
            Assert.InRange(results[i].Indicators["StochRsi_K"], 0m, 100m);
        }
    }

    [Fact]
    public void Stoch_WithFastKPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            StochasticIndicators.Stoch("Stoch", 0, 1, 1));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static EnrichedCandle[] CreateStochasticCandles() =>
    [
        MakeCandle(10m, 12m, 8m, 10m, dayOffset: 0),
        MakeCandle(13m, 14m, 9m, 13m, dayOffset: 1),
        MakeCandle(15m, 16m, 11m, 15m, dayOffset: 2),
        MakeCandle(12m, 15m, 10m, 12m, dayOffset: 3),
        MakeCandle(17m, 18m, 13m, 17m, dayOffset: 4),
    ];

    private static EnrichedCandle MakeCandle(
        decimal open,
        decimal high,
        decimal low,
        decimal close,
        long volume = 1000,
        int dayOffset = 0) => new(new Candle
    {
        Timestamp = BaseTime.AddDays(dayOffset),
        Open = open,
        High = high,
        Low = low,
        Close = close,
        Volume = volume,
    });
}
