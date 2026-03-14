using System.Threading.Tasks.Dataflow;
using OpenQuant.Analysis;
using OpenQuant.Models;

namespace OpenQuant.Tests.Analysis;

public class VolumeIndicatorsTests
{
    private static readonly DateTimeOffset BaseTime = new(2025, 1, 1, 0, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task Obv_WithMixedCloses_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var obv = VolumeIndicators.Obv("OBV");
        var sink = CreateSink(results);
        obv.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 11m, 9m, 10m, 100, 0),
            MakeCandle(12m, 13m, 11m, 12m, 200, 1),
            MakeCandle(11m, 12m, 10m, 11m, 150, 2),
            MakeCandle(13m, 14m, 12m, 13m, 300, 3),
        };

        foreach (var candle in candles)
        {
            await obv.SendAsync(candle);
        }

        obv.Complete();
        await sink.Completion;

        Assert.Equal(0m, results[0].Indicators["OBV"]);
        Assert.Equal(200m, results[1].Indicators["OBV"]);
        Assert.Equal(50m, results[2].Indicators["OBV"]);
        Assert.Equal(350m, results[3].Indicators["OBV"]);
    }

    [Fact]
    public async Task Ad_WithValidCandle_ReturnsExpectedValue()
    {
        var results = new List<EnrichedCandle>();
        var ad = VolumeIndicators.Ad("AD");
        var sink = CreateSink(results);
        ad.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        await ad.SendAsync(MakeCandle(15m, 20m, 10m, 18m, 1000, 0));
        ad.Complete();
        await sink.Completion;

        Assert.Equal(600m, results[0].Indicators["AD"]);
    }

    [Fact]
    public async Task AdOsc_WithValidPeriods_ReturnsExpectedValues()
    {
        var results = new List<EnrichedCandle>();
        var adOsc = VolumeIndicators.AdOsc("ADOSC", 2, 3);
        var sink = CreateSink(results);
        adOsc.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(18m, 20m, 10m, 18m, 1000, 0),
            MakeCandle(16m, 22m, 12m, 16m, 1200, 1),
            MakeCandle(23m, 24m, 14m, 23m, 900, 2),
            MakeCandle(20m, 26m, 15m, 20m, 800, 3),
        };

        foreach (var candle in candles)
        {
            await adOsc.SendAsync(candle);
        }

        adOsc.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("ADOSC"));
        Assert.False(results[1].Indicators.ContainsKey("ADOSC"));
        Assert.Equal(200m, Math.Round(results[2].Indicators["ADOSC"], 2));
        Assert.Equal(121.21m, Math.Round(results[3].Indicators["ADOSC"], 2));
    }

    [Fact]
    public async Task Mfi_WithValidPeriod_ReturnsBoundedValues()
    {
        var results = new List<EnrichedCandle>();
        var mfi = VolumeIndicators.Mfi("MFI", 2);
        var sink = CreateSink(results);
        mfi.LinkTo(sink, new DataflowLinkOptions { PropagateCompletion = true });

        var candles = new[]
        {
            MakeCandle(10m, 12m, 8m, 10m, 100, 0),
            MakeCandle(12m, 14m, 10m, 12m, 120, 1),
            MakeCandle(14m, 15m, 11m, 14m, 140, 2),
            MakeCandle(10m, 13m, 9m, 10m, 160, 3),
        };

        foreach (var candle in candles)
        {
            await mfi.SendAsync(candle);
        }

        mfi.Complete();
        await sink.Completion;

        Assert.False(results[0].Indicators.ContainsKey("MFI"));
        Assert.False(results[1].Indicators.ContainsKey("MFI"));
        Assert.Equal(100m, results[2].Indicators["MFI"]);
        Assert.Equal(52.24m, Math.Round(results[3].Indicators["MFI"], 2));
        Assert.InRange(results[3].Indicators["MFI"], 0m, 100m);
    }

    [Fact]
    public void Mfi_WithPeriodLessThanOne_Throws()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            VolumeIndicators.Mfi("MFI", 0));
    }

    private static ActionBlock<EnrichedCandle> CreateSink(List<EnrichedCandle> results) =>
        new(item => results.Add(item));

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
