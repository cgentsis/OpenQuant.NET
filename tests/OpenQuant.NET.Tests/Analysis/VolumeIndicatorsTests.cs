using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class VolumeIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Obv_ProducesValueOnFirstCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = VolumeIndicators.Obv("OBV");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetVolumeCandles().Take(1));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.True(results[0].Indicators.ContainsKey("OBV"));
        Assert.Equal(0m, results[0].Indicators["OBV"]);
    }

    [Fact]
    public async Task Obv_IncreasesWhenCloseRises()
    {
        var results = new List<EnrichedCandle>();
        var block = VolumeIndicators.Obv("OBV");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetVolumeCandles().Take(3));
        block.Complete();
        await sink.Completion;

        Assert.True(results[1].Indicators["OBV"] > results[0].Indicators["OBV"]);
        Assert.True(results[2].Indicators["OBV"] > results[1].Indicators["OBV"]);
    }

    [Fact]
    public async Task Ad_ProducesValueOnFirstCandle()
    {
        var results = new List<EnrichedCandle>();
        var block = VolumeIndicators.Ad("AD");
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetVolumeCandles().Take(1));
        block.Complete();
        await sink.Completion;

        Assert.Single(results);
        Assert.True(results[0].Indicators.ContainsKey("AD"));
    }

    [Fact]
    public async Task AdOsc_ProducesValueAfterSlowPeriodCandles()
    {
        var results = new List<EnrichedCandle>();
        var block = VolumeIndicators.AdOsc("ADOSC", 3, 5);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetVolumeCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("ADOSC"));
        Assert.False(results[1].Indicators.ContainsKey("ADOSC"));
        Assert.False(results[2].Indicators.ContainsKey("ADOSC"));
        Assert.False(results[3].Indicators.ContainsKey("ADOSC"));
        Assert.True(results[4].Indicators.ContainsKey("ADOSC"));
    }

    [Fact]
    public async Task Mfi_ProducesValueAfterPeriodPlusOneCandles_AndStaysBounded()
    {
        var results = new List<EnrichedCandle>();
        var block = VolumeIndicators.Mfi("MFI", 3);
        var sink = CreateSink(results);
        block.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await SendCandles(block, GetVolumeCandles());
        block.Complete();
        await sink.Completion;

        Assert.Equal(10, results.Count);
        Assert.False(results[0].Indicators.ContainsKey("MFI"));
        Assert.False(results[1].Indicators.ContainsKey("MFI"));
        Assert.False(results[2].Indicators.ContainsKey("MFI"));
        Assert.True(results[3].Indicators.ContainsKey("MFI"));
        Assert.InRange(results[3].Indicators["MFI"], 0m, 100m);
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

    private static async Task SendCandles(ITargetBlock<EnrichedCandle> block, IEnumerable<EnrichedCandle> candles)
    {
        foreach (var candle in candles)
        {
            await block.SendAsync(candle);
        }
    }

    private static EnrichedCandle[] GetVolumeCandles() =>
    [
        MakeCandle(12m, 14m, 11m, 13m, 1000, 0),
        MakeCandle(13m, 16m, 12m, 15m, 1200, 1),
        MakeCandle(15m, 17m, 14m, 16m, 1400, 2),
        MakeCandle(16m, 18m, 15m, 17m, 1600, 3),
        MakeCandle(17m, 18m, 14m, 15m, 1800, 4),
        MakeCandle(15m, 17m, 13m, 14m, 2000, 5),
        MakeCandle(14m, 16m, 13m, 15m, 2200, 6),
        MakeCandle(15m, 19m, 14m, 18m, 2400, 7),
        MakeCandle(18m, 20m, 17m, 19m, 2600, 8),
        MakeCandle(19m, 21m, 18m, 20m, 2800, 9),
    ];

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
